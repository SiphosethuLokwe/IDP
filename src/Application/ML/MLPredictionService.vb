Imports Microsoft.ML
Imports System
Imports System.Threading.Tasks
Imports IDP.Domain.Entities

Namespace IDP.Application.ML

    ''' <summary>
    ''' ML.NET prediction service for real-time duplicate detection
    ''' Uses trained model to predict if two learners are duplicates
    ''' </summary>
    Public Class MLPredictionService

        Private ReadOnly _mlContext As MLContext
        Private ReadOnly _fuzzyMatching As FuzzyMatchingService
        Private _predictionEngine As PredictionEngine(Of LearnerComparisonInput, LearnerDuplicationPrediction)
        Private _isModelLoaded As Boolean

        Public Sub New(mlContext As MLContext, fuzzyMatching As FuzzyMatchingService)
            _mlContext = mlContext
            _fuzzyMatching = fuzzyMatching
            _isModelLoaded = False
        End Sub

        ''' <summary>
        ''' Initializes the prediction engine with a trained model
        ''' </summary>
        Public Sub LoadModel(trainedModel As ITransformer)
            _predictionEngine = _mlContext.Model.CreatePredictionEngine(Of LearnerComparisonInput, LearnerDuplicationPrediction)(trainedModel)
            _isModelLoaded = True
            Console.WriteLine("ML Prediction engine initialized")
        End Sub

        ''' <summary>
        ''' Main prediction method: Compares two learners and predicts if they're duplicates
        ''' Returns prediction with probability score and explanation
        ''' </summary>
        Public Function PredictDuplicate(learner1 As Learner, learner2 As Learner) As DuplicationExplanation
            If Not _isModelLoaded Then
                Throw New InvalidOperationException("ML model not loaded. Call LoadModel first.")
            End If

            ' Step 1: Extract features using fuzzy matching
            Dim features = ExtractFeatures(learner1, learner2)
            
            ' Step 2: Get ML prediction
            Dim prediction = _predictionEngine.Predict(features)
            
            ' Step 3: Build explanation
            Dim explanation = BuildExplanation(learner1, learner2, features, prediction)
            
            Return explanation
        End Function

        ''' <summary>
        ''' Extracts comparison features from two learners
        ''' These features feed into the ML model
        ''' </summary>
        Private Function ExtractFeatures(learner1 As Learner, learner2 As Learner) As LearnerComparisonInput
            Dim features = New LearnerComparisonInput()

            ' ID Number Similarity
            If Not String.IsNullOrEmpty(learner1.IdNumber) AndAlso Not String.IsNullOrEmpty(learner2.IdNumber) Then
                features.IdNumberSimilarity = _fuzzyMatching.CompareIdNumbers(learner1.IdNumber, learner2.IdNumber)
            Else
                features.IdNumberSimilarity = 0.0F
            End If

            ' First Name Similarity (using Jaro-Winkler for names)
            If Not String.IsNullOrEmpty(learner1.FirstName) AndAlso Not String.IsNullOrEmpty(learner2.FirstName) Then
                features.FirstNameSimilarity = _fuzzyMatching.JaroWinklerSimilarity(learner1.FirstName, learner2.FirstName)
            Else
                features.FirstNameSimilarity = 0.0F
            End If

            ' Last Name Similarity
            If Not String.IsNullOrEmpty(learner1.LastName) AndAlso Not String.IsNullOrEmpty(learner2.LastName) Then
                features.LastNameSimilarity = _fuzzyMatching.JaroWinklerSimilarity(learner1.LastName, learner2.LastName)
            Else
                features.LastNameSimilarity = 0.0F
            End If

            ' Phonetic Name Match (sounds-alike)
            If Not String.IsNullOrEmpty(learner1.LastName) AndAlso Not String.IsNullOrEmpty(learner2.LastName) Then
                features.PhoneticNameMatch = _fuzzyMatching.PhoneticMatch(learner1.LastName, learner2.LastName)
            Else
                features.PhoneticNameMatch = 0.0F
            End If

            ' Date of Birth Match
            If learner1.DateOfBirth.HasValue AndAlso learner2.DateOfBirth.HasValue Then
                If learner1.DateOfBirth.Value = learner2.DateOfBirth.Value Then
                    features.DobMatch = 1.0F ' Exact match
                ElseIf learner1.DateOfBirth.Value.Year = learner2.DateOfBirth.Value.Year Then
                    features.DobMatch = 0.5F ' Same year (possible typo in month/day)
                Else
                    features.DobMatch = 0.0F
                End If
            Else
                features.DobMatch = 0.0F
            End If

            ' Phone Number Similarity
            If Not String.IsNullOrEmpty(learner1.PhoneNumber) AndAlso Not String.IsNullOrEmpty(learner2.PhoneNumber) Then
                features.PhoneSimilarity = _fuzzyMatching.CalculateSimilarity(
                    CleanPhoneNumber(learner1.PhoneNumber),
                    CleanPhoneNumber(learner2.PhoneNumber))
            Else
                features.PhoneSimilarity = 0.0F
            End If

            ' Email Similarity
            If Not String.IsNullOrEmpty(learner1.Email) AndAlso Not String.IsNullOrEmpty(learner2.Email) Then
                features.EmailSimilarity = _fuzzyMatching.CalculateSimilarity(
                    learner1.Email.ToLower(),
                    learner2.Email.ToLower())
            Else
                features.EmailSimilarity = 0.0F
            End If

            ' Same SETA Code
            features.SameSetaCode = If(learner1.SetaCode = learner2.SetaCode, 1.0F, 0.0F)

            ' Has Active Contract
            features.HasActiveContract = If(learner1.HasActiveContract() OrElse learner2.HasActiveContract(), 1.0F, 0.0F)

            ' Contract Overlap (check if they have overlapping active contracts)
            features.ContractOverlap = If(CheckContractOverlap(learner1, learner2), 1.0F, 0.0F)

            Return features
        End Function

        ''' <summary>
        ''' Builds human-readable explanation of why learners were flagged as duplicates
        ''' Critical for transparency and admin decision-making
        ''' </summary>
        Private Function BuildExplanation(
            learner1 As Learner,
            learner2 As Learner,
            features As LearnerComparisonInput,
            prediction As LearnerDuplicationPrediction) As DuplicationExplanation

            Dim explanation = New DuplicationExplanation() With {
                .OverallConfidence = CDec(prediction.Probability),
                .IsDuplicate = prediction.PredictedLabel
            }

            ' Add matching factors with weights
            If features.IdNumberSimilarity > 0.5F Then
                explanation.MatchingFactors.Add(New MatchFactor() With {
                    .FactorName = "ID Number",
                    .Similarity = CDec(features.IdNumberSimilarity),
                    .Weight = 0.35D,
                    .Description = If(features.IdNumberSimilarity = 1.0F, "Exact match", $"Similar ({features.IdNumberSimilarity:P0})"),
                    .Value1 = learner1.IdNumber,
                    .Value2 = learner2.IdNumber
                })
            End If

            If features.DobMatch > 0.0F Then
                explanation.MatchingFactors.Add(New MatchFactor() With {
                    .FactorName = "Date of Birth",
                    .Similarity = CDec(features.DobMatch),
                    .Weight = 0.25D,
                    .Description = If(features.DobMatch = 1.0F, "Exact match", "Same year"),
                    .Value1 = If(learner1.DateOfBirth.HasValue, learner1.DateOfBirth.Value.ToString("yyyy-MM-dd"), "N/A"),
                    .Value2 = If(learner2.DateOfBirth.HasValue, learner2.DateOfBirth.Value.ToString("yyyy-MM-dd"), "N/A")
                })
            End If

            If features.FirstNameSimilarity > 0.7F OrElse features.LastNameSimilarity > 0.7F Then
                Dim nameScore = (features.FirstNameSimilarity + features.LastNameSimilarity) / 2
                explanation.MatchingFactors.Add(New MatchFactor() With {
                    .FactorName = "Name",
                    .Similarity = CDec(nameScore),
                    .Weight = 0.2D,
                    .Description = $"Names are {nameScore:P0} similar",
                    .Value1 = $"{learner1.FirstName} {learner1.LastName}",
                    .Value2 = $"{learner2.FirstName} {learner2.LastName}"
                })
            End If

            If features.PhoneticNameMatch > 0.8F Then
                explanation.MatchingFactors.Add(New MatchFactor() With {
                    .FactorName = "Phonetic Match",
                    .Similarity = CDec(features.PhoneticNameMatch),
                    .Weight = 0.1D,
                    .Description = "Names sound alike (possible spelling variation)",
                    .Value1 = learner1.LastName,
                    .Value2 = learner2.LastName
                })
            End If

            If features.PhoneSimilarity > 0.8F Then
                explanation.MatchingFactors.Add(New MatchFactor() With {
                    .FactorName = "Phone Number",
                    .Similarity = CDec(features.PhoneSimilarity),
                    .Weight = 0.05D,
                    .Description = If(features.PhoneSimilarity = 1.0F, "Exact match", "Very similar"),
                    .Value1 = learner1.PhoneNumber,
                    .Value2 = learner2.PhoneNumber
                })
            End If

            If features.EmailSimilarity > 0.8F Then
                explanation.MatchingFactors.Add(New MatchFactor() With {
                    .FactorName = "Email",
                    .Similarity = CDec(features.EmailSimilarity),
                    .Weight = 0.05D,
                    .Description = If(features.EmailSimilarity = 1.0F, "Exact match", "Very similar"),
                    .Value1 = learner1.Email,
                    .Value2 = learner2.Email
                })
            End If

            ' Determine risk level and recommended action
            If explanation.OverallConfidence >= 0.9D Then
                explanation.RiskLevel = "HIGH"
                explanation.RecommendedAction = "BLOCK - Very high confidence duplicate"
            ElseIf explanation.OverallConfidence >= 0.75D Then
                explanation.RiskLevel = "MEDIUM"
                explanation.RecommendedAction = "FLAG - Requires manual review"
            ElseIf explanation.OverallConfidence >= 0.5D Then
                explanation.RiskLevel = "LOW"
                explanation.RecommendedAction = "MONITOR - Possible match, low confidence"
            Else
                explanation.RiskLevel = "NONE"
                explanation.RecommendedAction = "ALLOW - Not a duplicate"
            End If

            Return explanation
        End Function

        Private Function CleanPhoneNumber(phone As String) As String
            If String.IsNullOrEmpty(phone) Then Return String.Empty
            ' Remove common formatting: spaces, dashes, parentheses
            Return phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "")
        End Function

        Private Function CheckContractOverlap(learner1 As Learner, learner2 As Learner) As Boolean
            Dim contracts1 = learner1.GetActiveContracts()
            Dim contracts2 = learner2.GetActiveContracts()

            If contracts1 Is Nothing OrElse contracts2 Is Nothing Then Return False

            For Each c1 In contracts1
                For Each c2 In contracts2
                    If c1.OverlapsWith(c2) Then Return True
                Next
            Next

            Return False
        End Function

    End Class

End Namespace
