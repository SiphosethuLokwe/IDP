Imports System
Imports IDP.Domain.Enums

Namespace DTOs
    Public Class DuplicationCheckResultDto
        Public Property HasDuplicates As Boolean
        Public Property TotalMatches As Integer
        Public Property Matches As List(Of DuplicateMatchDto)
        
        Public Sub New()
            Matches = New List(Of DuplicateMatchDto)()
        End Sub
    End Class
    
    Public Class DuplicateMatchDto
        Public Property MatchedLearnerId As Guid
        Public Property MatchType As MatchType
        Public Property ConfidenceScore As Decimal
        Public Property MatchedFields As List(Of String)
        Public Property LearnerDetails As LearnerDto
        
        Public Sub New()
            MatchedFields = New List(Of String)()
        End Sub
    End Class
End Namespace
