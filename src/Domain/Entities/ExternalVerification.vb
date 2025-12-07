Imports System
Imports IDP.Domain.Enums

Namespace Entities
    Public Class ExternalVerification
        Public Property Id As Guid
        Public Property LearnerId As Guid
        Public Property VerificationType As VerificationType
        Public Property VerificationProvider As String
        Public Property RequestPayload As String
        Public Property ResponsePayload As String
        Public Property IsVerified As Boolean
        Public Property VerificationStatus As String
        Public Property VerifiedAt As DateTime
        Public Property ErrorMessage As String
        
        ' Navigation properties
        Public Property Learner As Learner
        
        Public Sub New()
            Id = Guid.NewGuid()
            VerifiedAt = DateTime.UtcNow
        End Sub
    End Class
End Namespace
