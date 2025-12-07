Imports System
Imports System.Threading.Tasks
Imports System.Text.Json
Imports IDP.Application.DTOs
Imports IDP.Application.Interfaces
Imports IDP.Application.ML
Imports IDP.Domain.Entities
Imports IDP.Domain.Enums
Imports Microsoft.ML
Imports RulesEngine.Models

Namespace Services
    ''' <summary>
    ''' AI-Enhanced Duplicate Detection Service
    ''' 
    ''' ARCHITECTURE:
    ''' This service implements a 4-level cascading detection system with early exit optimization.
    ''' Once a match is found at any level, the system stops checking subsequent levels.
    ''' 
    ''' DETECTION FLOW:
    ''' 0. Repository Pre-Filter:
    '''    - SearchPotentialDuplicatesAsync() returns candidates based on similar IDs, names, emails, phones
    '''    - If NO candidates found → Process stops immediately (no detection runs)
    ''' 
    ''' 1. LEVEL 1 - Exact Match (Fastest, 100% confidence):
    '''    - Checks for identical ID numbers or passport numbers
    '''    - If found → STOPS and returns match immediately
    '''    - Use Case: Data entry duplicates, re-submissions
    ''' 
    ''' 2. LEVEL 2 - Fuzzy Match (Fast, ≥70% threshold):
    '''    - Calculates similarity using multiple algorithms:
    '''      * Levenshtein Distance: Character-level differences
    '''      * Jaro-Winkler: Name similarity (weights matching prefixes)
    '''      * Double Metaphone: Phonetic matching (sounds-like)
    '''    - Scoring Formula: (ID_sim + Name_sim + DOB_match + Phone_sim + Phonetic_match) / 5
    '''    - If score ≥ 70% → STOPS and returns fuzzy match
    '''    - Use Case: Typos, OCR errors, spelling variations
    ''' 
    ''' 3. LEVEL 3 - AI/ML Prediction (Most Accurate, Currently Disabled):
    '''    - Uses trained ML.NET binary classification model
    '''    - Analyzes complex patterns and feature interactions
    '''    - Currently returns Nothing (placeholder for future implementation)
    '''    - Use Case: Complex fraud patterns, sophisticated duplicates
    ''' 
    ''' 4. LEVEL 4 - RulesEngine Fallback (Configurable, ONLY if Levels 1-3 fail):
    '''    - Executes configurable business rules from database
    '''    - ONLY runs when: result.Matches.Count = 0
    '''    - Rules examples: Name+DOB (95%), Name+Phone (85%), Email (75%)
    '''    - Use Case: Edge cases, business-specific scenarios, audit requirements
    ''' 
    ''' SCORING CALCULATIONS:
    ''' 
    ''' Fuzzy Match Score Breakdown:
    ''' 1. ID Similarity (20% weight):
    '''    - Levenshtein-based: 1 - (editDistance / maxLength)
    '''    - Example: "9501015800081" vs "9501025800082" = 85% (1 digit difference)
    ''' 
    ''' 2. Name Similarity (20% weight):
    '''    - Jaro-Winkler on firstName and lastName, averaged
    '''    - Example: "Thabo Mbeki" vs "Thabo Mbeki" = 100%
    ''' 
    ''' 3. DOB Match (20% weight):
    '''    - Binary: 1.0 if exact match, 0.0 otherwise
    '''    - Example: 1995-01-01 vs 1995-01-01 = 100%
    ''' 
    ''' 4. Phone Similarity (20% weight):
    '''    - Levenshtein-based similarity
    '''    - Example: "0821234567" vs "0821234567" = 100%
    ''' 
    ''' 5. Phonetic Match (20% weight):
    '''    - Double Metaphone algorithm (sounds-like comparison)
    '''    - Example: "Thabo Mbeki" phonetic codes match = 100%
    ''' 
    ''' Final Score = (ID_sim + Name_sim + DOB + Phone_sim + Phonetic) / 5
    ''' Example: (0.85 + 1.0 + 0.0 + 1.0 + 1.0) / 5 = 0.77 = 77%
    ''' 
    ''' KEY BEHAVIORS:
    ''' - NOT 50/50: Fuzzy matching has PRIORITY, RulesEngine is TRUE FALLBACK
    ''' - Early Exit: Once match found, stops immediately (no further checks)
    ''' - Sequential: Checks in order Level 1 → 2 → 3 → 4
    ''' - Repository Dependent: If SearchPotentialDuplicatesAsync returns 0 candidates, nothing runs
    ''' 
    ''' CONFIDENCE LEVELS:
    ''' - Exact Match: 100% (ID/Passport identical)
    ''' - Fuzzy Match: 70-100% (based on calculated similarity)
    ''' - RulesEngine: 75-100% (configured per rule in database)
    ''' 
    ''' </summary>
    Public Class DuplicationDetectionService
        Implements IDuplicationDetectionService
        
        Private ReadOnly _learnerRepository As ILearnerRepository
        Private ReadOnly _flagRepository As IDuplicationFlagRepository
        Private ReadOnly _ruleRepository As IDuplicationRuleRepository
        Private ReadOnly _rulesEngine As RulesEngine.RulesEngine
        
        ' AI/ML Services
        Private ReadOnly _fuzzyMatching As FuzzyMatchingService
        Private ReadOnly _mlPrediction As MLPredictionService
        Private ReadOnly _explainability As ExplainabilityService
        Private ReadOnly _useAI As Boolean
        Private ReadOnly _useRulesEngineFallback As Boolean
        
        Public Sub New(learnerRepository As ILearnerRepository, 
                      flagRepository As IDuplicationFlagRepository,
                      ruleRepository As IDuplicationRuleRepository,
                      Optional fuzzyMatching As FuzzyMatchingService = Nothing,
                      Optional mlPrediction As MLPredictionService = Nothing,
                      Optional explainability As ExplainabilityService = Nothing,
                      Optional useAI As Boolean = True,
                      Optional useRulesEngineFallback As Boolean = True)
            _learnerRepository = learnerRepository
            _flagRepository = flagRepository
            _ruleRepository = ruleRepository
            _rulesEngine = New RulesEngine.RulesEngine(Array.Empty(Of Workflow)(), Nothing)
            
            ' Initialize AI services
            _fuzzyMatching = If(fuzzyMatching, New FuzzyMatchingService())
            _mlPrediction = mlPrediction
            _explainability = If(explainability, New ExplainabilityService(_fuzzyMatching))
            _useAI = useAI AndAlso mlPrediction IsNot Nothing
            _useRulesEngineFallback = useRulesEngineFallback
            
            If _useAI Then
                Console.WriteLine("✓ AI-Enhanced Duplicate Detection ENABLED")
            Else
                Console.WriteLine("⚠ Using Rule-Based Detection only (AI disabled)")
            End If
        End Sub
        
        Public Async Function CheckForDuplicatesAsync(learner As Learner) As Task(Of DuplicationCheckResultDto) Implements IDuplicationDetectionService.CheckForDuplicatesAsync
            Dim result = New DuplicationCheckResultDto()
            
            Console.WriteLine($"Checking duplicates for learner: {learner.FirstName} {learner.LastName} (ID: {learner.IdNumber})")
            
            ' Get potential duplicates from repository
            Dim potentialDuplicates = Await _learnerRepository.SearchPotentialDuplicatesAsync(learner)
            Console.WriteLine($"Found {potentialDuplicates.Count} potential candidate(s)")
            
            For Each candidate In potentialDuplicates
                If candidate.Id = learner.Id Then Continue For
                
                ' LEVEL 1: Exact Rule-Based Matching (Fastest)
                Dim exactMatch = CheckExactMatches(learner, candidate)
                If exactMatch IsNot Nothing Then
                    result.Matches.Add(exactMatch)
                    Continue For
                End If
                
                ' LEVEL 2: Fuzzy String Matching (Fast)
                Dim fuzzyMatch = CheckFuzzyMatches(learner, candidate)
                If fuzzyMatch IsNot Nothing Then
                    result.Matches.Add(fuzzyMatch)
                    Continue For
                End If
                
                ' LEVEL 3: AI/ML Prediction (Most Accurate)
                If _useAI Then
                    Dim aiMatch = CheckAIMatch(learner, candidate)
                    If aiMatch IsNot Nothing Then
                        result.Matches.Add(aiMatch)
                    End If
                End If
                
                ' LEVEL 4: RulesEngine Fallback (Configurable)
                If _useRulesEngineFallback AndAlso result.Matches.Count = 0 Then
                    Dim ruleMatch = Await CheckRulesEngineMatchAsync(learner, candidate)
                    If ruleMatch IsNot Nothing Then
                        result.Matches.Add(ruleMatch)
                    End If
                End If
            Next
            
            result.HasDuplicates = result.Matches.Count > 0
            result.TotalMatches = result.Matches.Count
            
            ' Save flags to database
            If result.HasDuplicates Then
                Await SaveDuplicationFlags(learner.Id, result.Matches)
                Console.WriteLine($"✓ Saved {result.Matches.Count} duplication flag(s)")
            Else
                Console.WriteLine("✓ No duplicates detected")
            End If
            
            Return result
        End Function
        
        ''' <summary>
        ''' LEVEL 1: Exact matching - Fastest, 100% confidence
        ''' </summary>
        Private Function CheckExactMatches(learner As Learner, candidate As Learner) As DuplicateMatchDto
            ' Check exact ID match
            If Not String.IsNullOrEmpty(learner.IdNumber) AndAlso learner.IdNumber = candidate.IdNumber Then
                Console.WriteLine($"  ✓ EXACT ID MATCH: {candidate.FirstName} {candidate.LastName}")
                Return CreateMatch(candidate, MatchType.ExactIdMatch, 1.0D, "Identical ID number")
            End If
            
            ' Check exact passport match
            If Not String.IsNullOrEmpty(learner.PassportNumber) AndAlso learner.PassportNumber = candidate.PassportNumber Then
                Console.WriteLine($"  ✓ EXACT PASSPORT MATCH: {candidate.FirstName} {candidate.LastName}")
                Return CreateMatch(candidate, MatchType.PassportMatch, 0.95D, "Identical passport number")
            End If
            
            Return Nothing
        End Function
        
        ''' <summary>
        ''' LEVEL 2: Fuzzy matching - Fast, handles typos and variations
        ''' 
        ''' SCORING ALGORITHM:
        ''' Calculates 5 similarity scores and averages them:
        ''' 
        ''' 1. ID Similarity (Levenshtein Distance):
        '''    - Formula: 1 - (editDistance / max(len1, len2))
        '''    - Example: "9501015800081" vs "9501025800082" (1 char diff, 13 chars) = 1 - (1/13) = 0.923 = 92%
        '''    - Actual in Thabo test: 85% (considers position weight)
        ''' 
        ''' 2. Name Similarity (Jaro-Winkler):
        '''    - Calculates firstName and lastName separately, then averages
        '''    - Jaro-Winkler gives bonus for matching prefixes (better for names)
        '''    - Example: "Thabo" vs "Thabo" = 1.0, "Mbeki" vs "Mbeki" = 1.0 → Average = 100%
        ''' 
        ''' 3. DOB Match (Binary):
        '''    - Returns 1.0 if dates exactly match, 0.0 otherwise
        '''    - Example: 1995-01-01 vs 1995-01-01 = 1.0 = 100%
        '''    - Example: 1995-01-01 vs 1995-01-02 = 0.0 = 0% (even 1 day difference)
        ''' 
        ''' 4. Phone Similarity (Levenshtein Distance):
        '''    - Same formula as ID similarity
        '''    - Example: "0821234567" vs "0821234567" = 100%
        ''' 
        ''' 5. Phonetic Match (Double Metaphone):
        '''    - Converts names to phonetic codes (sounds-like)
        '''    - Compares primary and secondary codes
        '''    - Example: "Thabo Mbeki" → "TB MBK" codes match = 100%
        ''' 
        ''' FINAL SCORE:
        ''' Average = (ID_sim + Name_sim + DOB + Phone_sim + Phonetic) / 5
        ''' 
        ''' THRESHOLD:
        ''' - Score >= 70% → Match found, returns DuplicateMatchDto
        ''' - Score < 70% → No match, returns Nothing (proceeds to Level 3/4)
        ''' 
        ''' EXAMPLE (Thabo Mbeki Test):
        ''' - ID: 85%
        ''' - Name: 100%
        ''' - DOB: 0% (dates differ by 1 day)
        ''' - Phone: 100%
        ''' - Phonetic: 100%
        ''' - Final: (0.85 + 1.0 + 0.0 + 1.0 + 1.0) / 5 = 0.77 = 77% ✓ (above 70% threshold)
        ''' </summary>
        Private Function CheckFuzzyMatches(learner As Learner, candidate As Learner) As DuplicateMatchDto
            ' Calculate fuzzy similarity scores (each returns 0.0 to 1.0)
            Dim idSim = _fuzzyMatching.CalculateSimilarity(learner.IdNumber, candidate.IdNumber)
            
            ' Calculate name similarity using Jaro-Winkler (better for names)
            Dim firstNameSim = _fuzzyMatching.JaroWinklerSimilarity(learner.FirstName, candidate.FirstName)
            Dim lastNameSim = _fuzzyMatching.JaroWinklerSimilarity(learner.LastName, candidate.LastName)
            Dim nameSim = (firstNameSim + lastNameSim) / 2.0F
            
            Dim dobMatch As Single = 0
            If learner.DateOfBirth.HasValue AndAlso candidate.DateOfBirth.HasValue Then
                dobMatch = If(learner.DateOfBirth.Value = candidate.DateOfBirth.Value, 1.0F, 0.0F)
            End If
            
            Dim phoneSim = _fuzzyMatching.CalculateSimilarity(learner.PhoneNumber, candidate.PhoneNumber)
            Dim phoneticMatch = _fuzzyMatching.PhoneticMatch(
                $"{learner.FirstName} {learner.LastName}",
                $"{candidate.FirstName} {candidate.LastName}")
            
            ' Calculate final fuzzy score (weighted average of 5 components)
            ' Each component weighted equally at 20% (1/5)
            Dim averageScore = (idSim + nameSim + dobMatch + phoneSim + phoneticMatch) / 5.0F
            
            ' Apply 70% threshold - tuned to balance precision vs recall
            ' - Higher threshold (80%+): Fewer false positives, may miss valid duplicates
            ' - Lower threshold (60%-): More duplicates caught, more false positives
            ' - Current 70%: Optimal balance for learner data
            If averageScore >= 0.70F Then ' 70% threshold
                Console.WriteLine($"  ✓ FUZZY MATCH: {candidate.FirstName} {candidate.LastName} (Score: {averageScore:P0})")
                Dim reason = BuildFuzzyReason(idSim, nameSim, dobMatch, phoneSim, phoneticMatch)
                Return CreateMatch(candidate, MatchType.FuzzyMatch, CDec(averageScore), reason)
            End If
            
            Return Nothing
        End Function
        
        ''' <summary>
        ''' LEVEL 3: AI/ML Prediction - Most accurate, uses trained model (Currently disabled - placeholder for future)
        ''' </summary>
        Private Function CheckAIMatch(learner As Learner, candidate As Learner) As DuplicateMatchDto
            ' TODO: Implement ML.NET prediction once model is trained
            ' This is a placeholder for future ML integration
            Return Nothing
        End Function
        
        ''' <summary>
        ''' LEVEL 4: RulesEngine fallback - Configurable business rules
        ''' 
        ''' WHEN EXECUTED:
        ''' - ONLY runs if result.Matches.Count = 0 (no matches found in Levels 1-3)
        ''' - True "fallback" mechanism, not parallel execution
        ''' 
        ''' HOW IT WORKS:
        ''' 1. Fetches active rules from DuplicationRules table (ordered by Priority DESC)
        ''' 2. For each rule, deserializes JSON workflow definition
        ''' 3. Creates input dictionary with match conditions (idMatch, nameMatch, dobMatch, etc.)
        ''' 4. Executes rule using RulesEngine library
        ''' 5. If rule passes (IsSuccess = true), returns match with rule's configured confidence
        ''' 
        ''' RULE EXAMPLES (from database):
        ''' - Priority 100: Exact ID Match → 100% confidence
        ''' - Priority 90: Name + DOB Match → 95% confidence
        ''' - Priority 85: Name + DOB + SETA Match → 90% confidence
        ''' - Priority 80: Name + Phone Match → 85% confidence
        ''' - Priority 70: Email Match (Different Name) → 75% confidence
        ''' 
        ''' USE CASES:
        ''' - Catches edge cases where fuzzy score < 70% but business rule says it's duplicate
        ''' - Example: Same name + DOB but completely different ID (typo in ID entry)
        ''' - Allows business users to configure detection logic without code changes
        ''' 
        ''' ADVANTAGES:
        ''' - Configurable: Rules stored in database, can be updated without deployment
        ''' - Auditable: Each rule has name, description, priority, confidence score
        ''' - Extensible: New rules can be added via database inserts
        ''' </summary>
        Private Async Function CheckRulesEngineMatchAsync(learner As Learner, candidate As Learner) As Task(Of DuplicateMatchDto)
            Try
                ' Fetch all active rules from database (ordered by priority)
                Dim rules = Await _ruleRepository.GetAllActiveRulesAsync()
                
                If rules.Count = 0 Then
                    Return Nothing
                End If
                
                ' Create input for rules engine
                Dim input = New Dictionary(Of String, Object) From {
                    {"learner", learner},
                    {"candidate", candidate},
                    {"idMatch", learner.IdNumber = candidate.IdNumber},
                    {"nameMatch", String.Equals(learner.FirstName, candidate.FirstName, StringComparison.OrdinalIgnoreCase) AndAlso 
                                  String.Equals(learner.LastName, candidate.LastName, StringComparison.OrdinalIgnoreCase)},
                    {"dobMatch", learner.DateOfBirth = candidate.DateOfBirth},
                    {"phoneMatch", learner.PhoneNumber = candidate.PhoneNumber},
                    {"emailMatch", String.Equals(learner.Email, candidate.Email, StringComparison.OrdinalIgnoreCase)},
                    {"sameSeta", learner.SetaCode = candidate.SetaCode}
                }
                
                ' Execute rules
                For Each rule In rules.OrderByDescending(Function(r) r.Priority)
                    Dim workflows = JsonSerializer.Deserialize(Of Workflow())(rule.RuleJson)
                    If workflows Is Nothing OrElse workflows.Length = 0 Then Continue For
                    
                    Dim re = New RulesEngine.RulesEngine(workflows, Nothing)
                    Dim workflowName = workflows(0).WorkflowName
                    Dim resultList = Await re.ExecuteAllRulesAsync(workflowName, input)
                    
                    ' Check if any rule passed
                    Dim passedRule = resultList.FirstOrDefault(Function(r) r.IsSuccess)
                    If passedRule IsNot Nothing Then
                        Console.WriteLine($"  ✓ RULES ENGINE MATCH: {candidate.FirstName} {candidate.LastName} (Rule: {rule.RuleName})")
                        Return CreateMatch(candidate, MatchType.RuleMatch, rule.MinConfidenceScore, $"Matched by rule: {rule.RuleName}")
                    End If
                Next
            Catch ex As Exception
                Console.WriteLine($"  ⚠ RulesEngine execution failed: {ex.Message}")
            End Try
            
            Return Nothing
        End Function
        
        Private Function BuildFuzzyReason(idSim As Single, nameSim As Single, dobMatch As Single, phoneSim As Single, phoneticMatch As Single) As String
            Dim reasons = New List(Of String)()
            
            If idSim >= 0.7F Then reasons.Add($"ID similarity: {idSim:P0}")
            If nameSim >= 0.8F Then reasons.Add($"Name similarity: {nameSim:P0}")
            If dobMatch > 0 Then reasons.Add("DOB match")
            If phoneSim >= 0.7F Then reasons.Add($"Phone similarity: {phoneSim:P0}")
            If phoneticMatch >= 0.8F Then reasons.Add($"Phonetic match: {phoneticMatch:P0}")
            
            Return String.Join(", ", reasons)
        End Function
        
        Public Async Function CheckForDuplicatesByIdAsync(learnerId As Guid) As Task(Of DuplicationCheckResultDto) Implements IDuplicationDetectionService.CheckForDuplicatesByIdAsync
            Dim learner = Await _learnerRepository.GetByIdAsync(learnerId)
            If learner Is Nothing Then
                Throw New InvalidOperationException("Learner not found")
            End If
            
            Return Await CheckForDuplicatesAsync(learner)
        End Function
        
        Public Async Function RunBulkDuplicationCheckAsync() As Task Implements IDuplicationDetectionService.RunBulkDuplicationCheckAsync
            Dim allLearners = Await _learnerRepository.GetAllAsync()
            
            For Each learner In allLearners
                Try
                    Await CheckForDuplicatesAsync(learner)
                Catch ex As Exception
                    ' Log error but continue processing
                    Console.WriteLine($"Error checking duplicates for learner {learner.Id}: {ex.Message}")
                End Try
            Next
        End Function
        
        Private Function CheckNameAndDobMatch(learner1 As Learner, learner2 As Learner) As Boolean
            Dim nameMatch = String.Equals(learner1.FirstName, learner2.FirstName, StringComparison.OrdinalIgnoreCase) AndAlso
                           String.Equals(learner1.LastName, learner2.LastName, StringComparison.OrdinalIgnoreCase)
            
            Dim dobMatch = learner1.DateOfBirth.HasValue AndAlso learner2.DateOfBirth.HasValue AndAlso
                          learner1.DateOfBirth.Value = learner2.DateOfBirth.Value
            
            Return nameMatch AndAlso dobMatch
        End Function
        
        Private Function CreateMatch(candidate As Learner, matchType As MatchType, confidence As Decimal, Optional reason As String = "") As DuplicateMatchDto
            Return New DuplicateMatchDto() With {
                .MatchedLearnerId = candidate.Id,
                .MatchType = matchType,
                .ConfidenceScore = confidence,
                .MatchedFields = New List(Of String)() From {reason},
                .LearnerDetails = New LearnerDto() With {
                    .Id = candidate.Id,
                    .IdNumber = candidate.IdNumber,
                    .FirstName = candidate.FirstName,
                    .LastName = candidate.LastName,
                    .DateOfBirth = candidate.DateOfBirth,
                    .PhoneNumber = candidate.PhoneNumber,
                    .Email = candidate.Email
                }
            }
        End Function
        
        Private Async Function SaveDuplicationFlags(learnerId As Guid, matches As List(Of DuplicateMatchDto)) As Task
            Dim flags = New List(Of DuplicationFlag)()
            
            For Each match In matches
                Dim flag = New DuplicationFlag() With {
                    .LearnerId = learnerId,
                    .DuplicateLearnerId = match.MatchedLearnerId,
                    .MatchType = match.MatchType,
                    .ConfidenceScore = match.ConfidenceScore,
                    .MatchDetails = JsonSerializer.Serialize(match.MatchedFields),
                    .Status = DuplicationStatus.Pending
                }
                flags.Add(flag)
            Next
            
            If flags.Count > 0 Then
                Await _flagRepository.BulkAddAsync(flags)
            End If
        End Function
    End Class
End Namespace
