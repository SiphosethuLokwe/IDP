Imports System

Namespace Entities
    Public Class Learner
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
        Public Property CreatedAt As DateTime
        Public Property UpdatedAt As DateTime?
        Public Property IsActive As Boolean
        
        ' Navigation properties
        Public Property DuplicationFlags As List(Of DuplicationFlag)
        Public Property Contracts As List(Of Contract)
        
        Public Sub New()
            Id = Guid.NewGuid()
            CreatedAt = DateTime.UtcNow
            IsActive = True
            DuplicationFlags = New List(Of DuplicationFlag)()
            Contracts = New List(Of Contract)()
        End Sub
        
        ''' <summary>
        ''' Checks if learner has any active contracts across any SETA
        ''' </summary>
        Public Function HasActiveContract() As Boolean
            Return Contracts IsNot Nothing AndAlso 
                   Contracts.Any(Function(c) c.IsCurrentlyActive())
        End Function
        
        ''' <summary>
        ''' Gets all active contracts for the learner
        ''' </summary>
        Public Function GetActiveContracts() As List(Of Contract)
            If Contracts Is Nothing Then
                Return New List(Of Contract)()
            End If
            Return Contracts.Where(Function(c) c.IsCurrentlyActive()).ToList()
        End Function
    End Class
End Namespace
