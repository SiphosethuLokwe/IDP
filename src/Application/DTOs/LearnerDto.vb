Imports System

Namespace DTOs
    Public Class LearnerDto
        Public Property Id As Guid
        Public Property IdNumber As String
        Public Property FirstName As String
        Public Property LastName As String
        Public Property DateOfBirth As Date?
        Public Property PhoneNumber As String
        Public Property Email As String
        Public Property AlternativeIdNumber As String
        Public Property PassportNumber As String
        Public Property SetaCode As String
        Public Property OrganizationId As String
        Public Property IsActive As Boolean
    End Class
    
    Public Class CreateLearnerDto
        Public Property IdNumber As String
        Public Property FirstName As String
        Public Property LastName As String
        Public Property DateOfBirth As Date?
        Public Property PhoneNumber As String
        Public Property Email As String
        Public Property AlternativeIdNumber As String
        Public Property PassportNumber As String
        Public Property SetaCode As String
        Public Property OrganizationId As String
    End Class
    
    Public Class UpdateLearnerDto
        Public Property FirstName As String
        Public Property LastName As String
        Public Property PhoneNumber As String
        Public Property Email As String
        Public Property AlternativeIdNumber As String
        Public Property PassportNumber As String
        Public Property IsActive As Boolean
    End Class
End Namespace
