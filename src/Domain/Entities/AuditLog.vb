Imports System

Namespace Entities
    Public Class AuditLog
        Public Property Id As Guid
        Public Property EntityName As String
        Public Property EntityId As Guid
        Public Property Action As String
        Public Property PerformedBy As String
        Public Property PerformedAt As DateTime
        Public Property OldValues As String ' JSON
        Public Property NewValues As String ' JSON
        Public Property IpAddress As String
        
        Public Sub New()
            Id = Guid.NewGuid()
            PerformedAt = DateTime.UtcNow
        End Sub
    End Class
End Namespace
