Imports System

Namespace Entities
    Public Class DuplicationRule
        Public Property Id As Guid
        Public Property RuleName As String
        Public Property RuleDescription As String
        Public Property RuleJson As String ' JSON representation of the rule
        Public Property IsActive As Boolean
        Public Property Priority As Integer
        Public Property MinConfidenceScore As Decimal
        Public Property CreatedAt As DateTime
        Public Property UpdatedAt As DateTime?
        
        Public Sub New()
            Id = Guid.NewGuid()
            CreatedAt = DateTime.UtcNow
            IsActive = True
        End Sub
    End Class
End Namespace
