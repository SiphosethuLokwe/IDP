Imports System
Imports IDP.Domain.Enums

Namespace Entities
    Public Class DuplicationFlag
        Public Property Id As Guid
        Public Property LearnerId As Guid
        Public Property DuplicateLearnerId As Guid?
        Public Property MatchType As MatchType
        Public Property ConfidenceScore As Decimal
        Public Property MatchDetails As String ' JSON field with match details
        Public Property Status As DuplicationStatus
        Public Property ReviewedBy As String
        Public Property ReviewedAt As DateTime?
        Public Property CreatedAt As DateTime
        Public Property ResolvedAt As DateTime?
        Public Property Notes As String
        
        ' Navigation properties
        Public Property Learner As Learner
        Public Property DuplicateLearner As Learner
        
        Public Sub New()
            Id = Guid.NewGuid()
            CreatedAt = DateTime.UtcNow
            Status = DuplicationStatus.Pending
        End Sub
    End Class
End Namespace
