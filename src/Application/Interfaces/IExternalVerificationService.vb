Imports System.Threading.Tasks

Namespace Interfaces
    Public Interface IExternalVerificationService
        Function VerifyIdNumberAsync(idNumber As String) As Task(Of ExternalVerificationResult)
        Function VerifyPhoneNumberAsync(phoneNumber As String) As Task(Of ExternalVerificationResult)
        Function VerifyEmailAsync(email As String) As Task(Of ExternalVerificationResult)
    End Interface
    
    Public Class ExternalVerificationResult
        Public Property IsVerified As Boolean
        Public Property VerificationStatus As String
        Public Property ErrorMessage As String
        Public Property ResponseData As Object
        Public Property Provider As String
    End Class
End Namespace
