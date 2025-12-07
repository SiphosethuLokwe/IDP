Imports System
Imports IDP.Domain.Enums

Namespace DTOs
    Public Class DuplicationFlagDto
        Public Property Id As Guid
        Public Property LearnerId As Guid
        Public Property DuplicateLearnerId As Guid?
        Public Property MatchType As MatchType
        Public Property ConfidenceScore As Decimal
        Public Property MatchDetails As String
        Public Property Status As DuplicationStatus
        Public Property CreatedAt As DateTime
        Public Property ReviewedBy As String
        Public Property ReviewedAt As DateTime?
        Public Property Notes As String
        
        ' Include learner info for display
        Public Property LearnerName As String
        Public Property LearnerIdNumber As String
        Public Property DuplicateLearnerName As String
        Public Property DuplicateLearnerIdNumber As String
    End Class
    
    Public Class ReviewDuplicationDto
        Public Property FlagId As Guid
        Public Property Status As DuplicationStatus
        Public Property ReviewedBy As String
        Public Property Notes As String
    End Class
End Namespace
