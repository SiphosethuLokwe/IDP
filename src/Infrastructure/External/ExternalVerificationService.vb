Imports System
Imports System.Threading.Tasks
Imports IDP.Application.Interfaces
Imports IDP.Infrastructure.External

Namespace External
    Public Class ExternalVerificationService
        Implements IExternalVerificationService
        
        Private ReadOnly _checkIdApi As ICheckIdApi
        
        Public Sub New(checkIdApi As ICheckIdApi)
            _checkIdApi = checkIdApi
        End Sub
        
        Public Async Function VerifyIdNumberAsync(idNumber As String) As Task(Of ExternalVerificationResult) Implements IExternalVerificationService.VerifyIdNumberAsync
            Try
                Dim response = Await _checkIdApi.VerifyIdAsync(idNumber)
                
                Return New ExternalVerificationResult() With {
                    .IsVerified = response.IsValid,
                    .VerificationStatus = If(response.IsValid, "Verified", "Invalid"),
                    .ResponseData = response,
                    .Provider = "CheckID",
                    .ErrorMessage = response.ErrorMessage
                }
            Catch ex As Exception
                Return New ExternalVerificationResult() With {
                    .IsVerified = False,
                    .VerificationStatus = "Error",
                    .Provider = "CheckID",
                    .ErrorMessage = ex.Message
                }
            End Try
        End Function
        
        Public Async Function VerifyPhoneNumberAsync(phoneNumber As String) As Task(Of ExternalVerificationResult) Implements IExternalVerificationService.VerifyPhoneNumberAsync
            Try
                Dim response = Await _checkIdApi.ValidatePhoneAsync(phoneNumber)
                
                Return New ExternalVerificationResult() With {
                    .IsVerified = response.IsValid,
                    .VerificationStatus = If(response.IsValid, "Verified", "Invalid"),
                    .ResponseData = response,
                    .Provider = "CheckID",
                    .ErrorMessage = response.ErrorMessage
                }
            Catch ex As Exception
                Return New ExternalVerificationResult() With {
                    .IsVerified = False,
                    .VerificationStatus = "Error",
                    .Provider = "CheckID",
                    .ErrorMessage = ex.Message
                }
            End Try
        End Function
        
        Public Async Function VerifyEmailAsync(email As String) As Task(Of ExternalVerificationResult) Implements IExternalVerificationService.VerifyEmailAsync
            ' Placeholder for email verification
            ' In production, integrate with an email verification service
            Await Task.CompletedTask
            
            Return New ExternalVerificationResult() With {
                .IsVerified = True,
                .VerificationStatus = "NotImplemented",
                .Provider = "None",
                .ErrorMessage = "Email verification not yet implemented"
            }
        End Function
    End Class
End Namespace
