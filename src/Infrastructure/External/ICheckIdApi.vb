Imports System.Threading.Tasks
Imports Refit

Namespace External
    Public Interface ICheckIdApi
        <[Get]("/verify")>
        Function VerifyIdAsync(<Query> idNumber As String) As Task(Of CheckIdResponse)
        
        <[Get]("/validate-phone")>
        Function ValidatePhoneAsync(<Query> phoneNumber As String) As Task(Of PhoneValidationResponse)
    End Interface
    
    Public Class CheckIdResponse
        Public Property IsValid As Boolean
        Public Property IdNumber As String
        Public Property FirstName As String
        Public Property LastName As String
        Public Property DateOfBirth As String
        Public Property Gender As String
        Public Property ErrorMessage As String
    End Class
    
    Public Class PhoneValidationResponse
        Public Property IsValid As Boolean
        Public Property PhoneNumber As String
        Public Property Carrier As String
        Public Property Type As String
        Public Property ErrorMessage As String
    End Class
End Namespace
