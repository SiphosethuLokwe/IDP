Namespace IDP.Application.ML

    ''' <summary>
    ''' Input data for training the ML model
    ''' Each row represents a comparison between two learners
    ''' </summary>
    Public Class LearnerComparisonInput
        Public Property IdNumberSimilarity As Single ' 0.0 to 1.0
        Public Property FirstNameSimilarity As Single
        Public Property LastNameSimilarity As Single
        Public Property PhoneticNameMatch As Single
        Public Property DobMatch As Single ' 1.0 if exact, 0.5 if year matches, 0.0 if different
        Public Property PhoneSimilarity As Single
        Public Property EmailSimilarity As Single
        Public Property SameSetaCode As Single ' 1.0 if same, 0.0 if different
        Public Property HasActiveContract As Single ' 1.0 if target has active contract
        Public Property ContractOverlap As Single ' 1.0 if contracts overlap
        
        ''' <summary>
        ''' Label: True if these two learners are the same person (duplicate)
        ''' </summary>
        Public Property IsDuplicate As Boolean
    End Class

    ''' <summary>
    ''' Output prediction from the ML model
    ''' </summary>
    Public Class LearnerDuplicationPrediction
        ''' <summary>
        ''' Predicted label: Is this a duplicate?
        ''' </summary>
        Public Property PredictedLabel As Boolean
        
        ''' <summary>
        ''' Probability score (0.0 to 1.0) that this is a duplicate
        ''' </summary>
        Public Property Probability As Single
        
        ''' <summary>
        ''' Confidence score (0.0 to 1.0)
        ''' </summary>
        Public Property Score As Single
    End Class

    ''' <summary>
    ''' Detailed explanation of why two learners were flagged as duplicates
    ''' </summary>
    Public Class DuplicationExplanation
        Public Property OverallConfidence As Decimal
        Public Property IsDuplicate As Boolean
        Public Property RecommendedAction As String ' "BLOCK", "FLAG", "ALLOW"
        Public Property MatchingFactors As List(Of MatchFactor)
        Public Property RiskLevel As String ' "HIGH", "MEDIUM", "LOW"
        
        Public Sub New()
            MatchingFactors = New List(Of MatchFactor)()
        End Sub
    End Class

    Public Class MatchFactor
        Public Property FactorName As String
        Public Property Similarity As Decimal ' 0.0 to 1.0
        Public Property Weight As Decimal ' How important is this factor?
        Public Property Description As String
        Public Property Value1 As String ' Original value from learner 1
        Public Property Value2 As String ' Value from learner 2
    End Class

    ''' <summary>
    ''' Training metrics for the ML model
    ''' </summary>
    Public Class ModelMetrics
        Public Property Accuracy As Double
        Public Property Precision As Double
        Public Property Recall As Double
        Public Property F1Score As Double
        Public Property AucRoc As Double
        Public Property PositivePrecision As Double
        Public Property PositiveRecall As Double
        Public Property NegativePrecision As Double
        Public Property NegativeRecall As Double
        Public Property ConfusionMatrix As String
        Public Property TrainingDate As DateTime
        Public Property DatasetSize As Integer
    End Class

End Namespace
