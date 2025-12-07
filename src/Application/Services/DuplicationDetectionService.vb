Imports System
Imports System.Threading.Tasks
Imports System.Text.Json
Imports IDP.Application.DTOs
Imports IDP.Application.Interfaces
'Imports IDP.Application.ML
Imports IDP.Domain.Entities
Imports IDP.Domain.Enums
'Imports Microsoft.ML
Imports RulesEngine.Models

Namespace Services
    ''' <summary>
    ''' AI-Enhanced Duplicate Detection Service
    ''' Combines: Rule-based matching + Fuzzy algorithms + ML.NET predictions + Explainability
    ''' </summary>
    Public Class DuplicationDetectionService
        Implements IDuplicationDetectionService
        
        Private ReadOnly _learnerRepository As ILearnerRepository
        Private ReadOnly _flagRepository As IDuplicationFlagRepository
        Private ReadOnly _ruleRepository As IDuplicationRuleRepository
        Private ReadOnly _rulesEngine As RulesEngine.RulesEngine
        
        ' AI/ML Services
        'Private ReadOnly _fuzzyMatching As FuzzyMatchingService
        'Private ReadOnly _mlPrediction As MLPredictionService
        'Private ReadOnly _explainability As ExplainabilityService
        Private ReadOnly _useAI As Boolean
        
        Public Sub New(learnerRepository As ILearnerRepository, 
                      flagRepository As IDuplicationFlagRepository,
                      ruleRepository As IDuplicationRuleRepository,
                      Optional useAI As Boolean = False)
                      'Optional fuzzyMatching As FuzzyMatchingService = Nothing,
                      'Optional mlPrediction As MLPredictionService = Nothing,
                      'Optional explainability As ExplainabilityService = Nothing,
            _learnerRepository = learnerRepository
            _flagRepository = flagRepository
            _ruleRepository = ruleRepository
            _rulesEngine = New RulesEngine.RulesEngine(Array.Empty(Of Workflow)(), Nothing)
            
            ' Initialize AI services
            '_fuzzyMatching = If(fuzzyMatching, New FuzzyMatchingService())
            '_mlPrediction = mlPrediction
            '_explainability = If(explainability, New ExplainabilityService(_fuzzyMatching))
            _useAI = False 'useAI AndAlso mlPrediction IsNot Nothing
            
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
                
                ' LEVEL 2: Fuzzy String Matching (Fast) - DISABLED FOR NOW
                'Dim fuzzyMatch = CheckFuzzyMatches(learner, candidate)
                'If fuzzyMatch IsNot Nothing Then
                '    result.Matches.Add(fuzzyMatch)
                '    Continue For
                'End If
                
                ' LEVEL 3: AI/ML Prediction (Most Accurate) - DISABLED FOR NOW
                'If _useAI Then
                '    Dim aiMatch = CheckAIMatch(learner, candidate)
                '    If aiMatch IsNot Nothing Then
                '        result.Matches.Add(aiMatch)
                '    End If
                'End If
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
        ''' </summary>
        'Private Function CheckFuzzyMatches(learner As Learner, candidate As Learner) As DuplicateMatchDto
        '   ... TEMPORARILY DISABLED FOR MIGRATION ...
        'End Function
        
        ''' <summary>
        ''' LEVEL 3: AI/ML Prediction - Most accurate, uses trained model
        ''' </summary>
        'Private Function CheckAIMatch(learner As Learner, candidate As Learner) As DuplicateMatchDto
        '   ... TEMPORARILY DISABLED FOR MIGRATION ...
        'End Function
        
        'Private Function BuildFuzzyReason(idSim As Single, nameSim As Single, dobMatch As Single, phoneSim As Single, phoneticMatch As Single) As String
        '   ... TEMPORARILY DISABLED FOR MIGRATION ...
        'End Function
        
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
        
        'Private Function CreateMatchWithExplanation(candidate As Learner, matchType As MatchType, confidence As Decimal, reason As String, explanation As DuplicationExplanation) As DuplicateMatchDto
        '    Dim match = CreateMatch(candidate, matchType, confidence, reason)
        '    
        '    ' Add detailed explanation to matched fields
        '    For Each factor In explanation.MatchingFactors
        '        match.MatchedFields.Add($"{factor.FactorName}: {factor.Description}")
        '    Next
        '    
        '    Return match
        'End Function
        
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
