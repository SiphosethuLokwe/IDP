Imports System
Imports System.Linq
Imports IDP.Domain.Entities

Namespace IDP.Application.ML

    ''' <summary>
    ''' Explainability service - provides human-readable explanations of AI decisions
    ''' Critical for: Transparency, audit compliance, admin decision-making, POPIA requirements
    ''' </summary>
    Public Class ExplainabilityService

        Private ReadOnly _fuzzyMatching As FuzzyMatchingService

        Public Sub New(fuzzyMatching As FuzzyMatchingService)
            _fuzzyMatching = fuzzyMatching
        End Sub

        ''' <summary>
        ''' Generates a detailed, human-readable explanation of why two learners match
        ''' </summary>
        Public Function ExplainMatch(learner1 As Learner, learner2 As Learner, confidence As Decimal) As String
            Dim explanation = New System.Text.StringBuilder()
            
            explanation.AppendLine("=".PadRight(60, "="c))
            explanation.AppendLine("DUPLICATE DETECTION EXPLANATION")
            explanation.AppendLine("=".PadRight(60, "="c))
            explanation.AppendLine()
            
            explanation.AppendLine($"Overall Confidence: {confidence:P1}")
            explanation.AppendLine($"Decision: {GetDecision(confidence)}")
            explanation.AppendLine()
            
            explanation.AppendLine("MATCHING FACTORS:")
            explanation.AppendLine("-".PadRight(60, "-"c))
            
            ' ID Number Analysis
            If Not String.IsNullOrEmpty(learner1.IdNumber) AndAlso Not String.IsNullOrEmpty(learner2.IdNumber) Then
                Dim idSim = _fuzzyMatching.CompareIdNumbers(learner1.IdNumber, learner2.IdNumber)
                explanation.AppendLine($"ID Number:")
                explanation.AppendLine($"  Learner 1: {learner1.IdNumber}")
                explanation.AppendLine($"  Learner 2: {learner2.IdNumber}")
                explanation.AppendLine($"  Similarity: {idSim:P1} {GetMatchIcon(idSim)}")
                explanation.AppendLine($"  Analysis: {AnalyzeIdMatch(learner1.IdNumber, learner2.IdNumber, idSim)}")
                explanation.AppendLine()
            End If
            
            ' Name Analysis
            Dim nameSim = 0.0F
            If Not String.IsNullOrEmpty(learner1.FirstName) AndAlso Not String.IsNullOrEmpty(learner2.FirstName) Then
                nameSim = _fuzzyMatching.JaroWinklerSimilarity(
                    $"{learner1.FirstName} {learner1.LastName}",
                    $"{learner2.FirstName} {learner2.LastName}")
            End If
            
            explanation.AppendLine($"Name:")
            explanation.AppendLine($"  Learner 1: {learner1.FirstName} {learner1.LastName}")
            explanation.AppendLine($"  Learner 2: {learner2.FirstName} {learner2.LastName}")
            explanation.AppendLine($"  Similarity: {nameSim:P1} {GetMatchIcon(nameSim)}")
            
            ' Phonetic analysis
            If Not String.IsNullOrEmpty(learner1.LastName) AndAlso Not String.IsNullOrEmpty(learner2.LastName) Then
                Dim phonetic1 = _fuzzyMatching.GetPhoneticCode(learner1.LastName)
                Dim phonetic2 = _fuzzyMatching.GetPhoneticCode(learner2.LastName)
                If phonetic1 = phonetic2 Then
                    explanation.AppendLine($"  Note: Names sound alike (phonetic codes match: {phonetic1})")
                End If
            End If
            explanation.AppendLine()
            
            ' Date of Birth Analysis
            If learner1.DateOfBirth.HasValue AndAlso learner2.DateOfBirth.HasValue Then
                explanation.AppendLine($"Date of Birth:")
                explanation.AppendLine($"  Learner 1: {learner1.DateOfBirth.Value:yyyy-MM-dd}")
                explanation.AppendLine($"  Learner 2: {learner2.DateOfBirth.Value:yyyy-MM-dd}")
                
                If learner1.DateOfBirth.Value = learner2.DateOfBirth.Value Then
                    explanation.AppendLine($"  Match: EXACT ✓✓✓")
                ElseIf learner1.DateOfBirth.Value.Year = learner2.DateOfBirth.Value.Year Then
                    explanation.AppendLine($"  Match: Same year (possible typo in month/day)")
                Else
                    explanation.AppendLine($"  Match: Different")
                End If
                explanation.AppendLine()
            End If
            
            ' Contact Information
            If Not String.IsNullOrEmpty(learner1.PhoneNumber) AndAlso Not String.IsNullOrEmpty(learner2.PhoneNumber) Then
                Dim phoneSim = _fuzzyMatching.CalculateSimilarity(learner1.PhoneNumber, learner2.PhoneNumber)
                explanation.AppendLine($"Phone Number:")
                explanation.AppendLine($"  Learner 1: {learner1.PhoneNumber}")
                explanation.AppendLine($"  Learner 2: {learner2.PhoneNumber}")
                explanation.AppendLine($"  Similarity: {phoneSim:P1} {GetMatchIcon(phoneSim)}")
                explanation.AppendLine()
            End If
            
            If Not String.IsNullOrEmpty(learner1.Email) AndAlso Not String.IsNullOrEmpty(learner2.Email) Then
                Dim emailSim = _fuzzyMatching.CalculateSimilarity(learner1.Email, learner2.Email)
                explanation.AppendLine($"Email:")
                explanation.AppendLine($"  Learner 1: {learner1.Email}")
                explanation.AppendLine($"  Learner 2: {learner2.Email}")
                explanation.AppendLine($"  Similarity: {emailSim:P1} {GetMatchIcon(emailSim)}")
                explanation.AppendLine()
            End If
            
            ' SETA and Contract Information
            explanation.AppendLine($"SETA Information:")
            explanation.AppendLine($"  Learner 1 SETA: {If(String.IsNullOrEmpty(learner1.SetaCode), "N/A", learner1.SetaCode)}")
            explanation.AppendLine($"  Learner 2 SETA: {If(String.IsNullOrEmpty(learner2.SetaCode), "N/A", learner2.SetaCode)}")
            explanation.AppendLine($"  Same SETA: {If(learner1.SetaCode = learner2.SetaCode, "Yes ⚠️", "No")}")
            explanation.AppendLine()
            
            ' Active Contracts
            Dim activeContracts1 = learner1.GetActiveContracts()
            Dim activeContracts2 = learner2.GetActiveContracts()
            
            explanation.AppendLine($"Active Contracts:")
            explanation.AppendLine($"  Learner 1: {If(activeContracts1 IsNot Nothing, activeContracts1.Count, 0)} active contract(s)")
            explanation.AppendLine($"  Learner 2: {If(activeContracts2 IsNot Nothing, activeContracts2.Count, 0)} active contract(s)")
            
            If activeContracts1?.Count > 0 AndAlso activeContracts2?.Count > 0 Then
                explanation.AppendLine($"  ⚠️ WARNING: Both learners have active contracts!")
                explanation.AppendLine($"  This may indicate double funding if they are the same person.")
            End If
            explanation.AppendLine()
            
            ' Final Recommendation
            explanation.AppendLine("=".PadRight(60, "="c))
            explanation.AppendLine("RECOMMENDATION:")
            explanation.AppendLine("-".PadRight(60, "-"c))
            explanation.AppendLine(GetRecommendation(confidence, learner1, learner2))
            explanation.AppendLine("=".PadRight(60, "="c))
            
            Return explanation.ToString()
        End Function

        ''' <summary>
        ''' Generates a summary explanation in one sentence
        ''' </summary>
        Public Function GetShortExplanation(learner1 As Learner, learner2 As Learner, confidence As Decimal) As String
            Dim reasons = New List(Of String)()
            
            ' ID match
            If Not String.IsNullOrEmpty(learner1.IdNumber) AndAlso learner1.IdNumber = learner2.IdNumber Then
                reasons.Add("identical ID numbers")
            End If
            
            ' DOB match
            If learner1.DateOfBirth.HasValue AndAlso learner2.DateOfBirth.HasValue AndAlso 
               learner1.DateOfBirth.Value = learner2.DateOfBirth.Value Then
                reasons.Add("same date of birth")
            End If
            
            ' Name similarity
            If Not String.IsNullOrEmpty(learner1.FirstName) AndAlso Not String.IsNullOrEmpty(learner2.FirstName) Then
                Dim nameSim = _fuzzyMatching.JaroWinklerSimilarity(
                    $"{learner1.FirstName} {learner1.LastName}",
                    $"{learner2.FirstName} {learner2.LastName}")
                If nameSim > 0.85 Then
                    reasons.Add($"very similar names ({nameSim:P0})")
                End If
            End If
            
            ' Phone match
            If Not String.IsNullOrEmpty(learner1.PhoneNumber) AndAlso learner1.PhoneNumber = learner2.PhoneNumber Then
                reasons.Add("same phone number")
            End If
            
            If reasons.Count = 0 Then
                Return $"Low confidence match ({confidence:P0}) - insufficient matching data"
            End If
            
            Return $"Match detected ({confidence:P0}): {String.Join(", ", reasons)}"
        End Function

        Private Function GetMatchIcon(similarity As Single) As String
            If similarity >= 0.95F Then Return "✓✓✓ EXACT"
            If similarity >= 0.85F Then Return "✓✓ HIGH"
            If similarity >= 0.70F Then Return "✓ MEDIUM"
            If similarity >= 0.50F Then Return "~ LOW"
            Return "✗ NO MATCH"
        End Function

        Private Function GetDecision(confidence As Decimal) As String
            If confidence >= 0.9D Then Return "BLOCK - High confidence duplicate detected"
            If confidence >= 0.75D Then Return "FLAG - Manual review required"
            If confidence >= 0.5D Then Return "MONITOR - Possible duplicate, low confidence"
            Return "ALLOW - Not a duplicate"
        End Function

        Private Function AnalyzeIdMatch(id1 As String, id2 As String, similarity As Single) As String
            If similarity = 1.0F Then
                Return "Exact match - Same person or data entry duplication"
            ElseIf similarity > 0.9F Then
                Dim distance = _fuzzyMatching.LevenshteinDistance(id1, id2)
                Return $"Very similar ({distance} character(s) different) - Possible typo or transposition"
            ElseIf similarity > 0.7F Then
                Return "Moderately similar - Could be related persons or data error"
            Else
                Return "Different ID numbers"
            End If
        End Function

        Private Function GetRecommendation(confidence As Decimal, learner1 As Learner, learner2 As Learner) As String
            Dim recommendation = New System.Text.StringBuilder()
            
            If confidence >= 0.9D Then
                recommendation.AppendLine("🚫 BLOCK REGISTRATION")
                recommendation.AppendLine()
                recommendation.AppendLine("Reason: High confidence duplicate detected (≥90%)")
                recommendation.AppendLine()
                recommendation.AppendLine("Action Required:")
                recommendation.AppendLine("1. Do NOT allow new registration")
                recommendation.AppendLine("2. Notify administrator immediately")
                recommendation.AppendLine("3. Investigate for potential fraud")
                recommendation.AppendLine("4. Check for active funding in multiple SETAs")
                
            ElseIf confidence >= 0.75D Then
                recommendation.AppendLine("⚠️ FLAG FOR MANUAL REVIEW")
                recommendation.AppendLine()
                recommendation.AppendLine("Reason: Medium-high confidence match (75-89%)")
                recommendation.AppendLine()
                recommendation.AppendLine("Action Required:")
                recommendation.AppendLine("1. Allow registration but FLAG for review")
                recommendation.AppendLine("2. Administrator must review within 48 hours")
                recommendation.AppendLine("3. Verify learner identity documents")
                recommendation.AppendLine("4. Contact learner for clarification if needed")
                
            ElseIf confidence >= 0.5D Then
                recommendation.AppendLine("👁️ MONITOR")
                recommendation.AppendLine()
                recommendation.AppendLine("Reason: Low-medium confidence (50-74%)")
                recommendation.AppendLine()
                recommendation.AppendLine("Action Required:")
                recommendation.AppendLine("1. Allow registration")
                recommendation.AppendLine("2. Log for future pattern analysis")
                recommendation.AppendLine("3. Include in monthly reconciliation report")
                
            Else
                recommendation.AppendLine("✅ ALLOW REGISTRATION")
                recommendation.AppendLine()
                recommendation.AppendLine("Reason: Low confidence (<50%) - Likely different persons")
                recommendation.AppendLine()
                recommendation.AppendLine("Action: Proceed with normal registration process")
            End If
            
            Return recommendation.ToString()
        End Function

        ''' <summary>
        ''' Generates a JSON-friendly explanation for API responses
        ''' </summary>
        Public Function GetStructuredExplanation(explanation As DuplicationExplanation) As Object
            Return New With {
                .confidence = explanation.OverallConfidence,
                .isDuplicate = explanation.IsDuplicate,
                .riskLevel = explanation.RiskLevel,
                .recommendedAction = explanation.RecommendedAction,
                .matchingFactors = explanation.MatchingFactors.Select(Function(f) New With {
                    .factor = f.FactorName,
                    .similarity = f.Similarity,
                    .weight = f.Weight,
                    .description = f.Description,
                    .comparison = New With {
                        .value1 = f.Value1,
                        .value2 = f.Value2
                    }
                }).ToList()
            }
        End Function

    End Class

End Namespace
