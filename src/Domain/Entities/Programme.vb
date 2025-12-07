Imports System

Namespace Entities
    ''' <summary>
    ''' Represents a learnership, qualification, or skills programme offered by a SETA.
    ''' Based on NQF (National Qualifications Framework) standards.
    ''' </summary>
    Public Class Programme
        Public Property Id As Guid
        Public Property QualificationCode As String ' e.g., "49001", SAQA ID
        Public Property Title As String
        Public Property Description As String
        Public Property NQFLevel As Integer ' 1-10
        Public Property Credits As Integer
        Public Property Sector As String
        Public Property ProgrammeType As String ' Learnership, Apprenticeship, Skills Programme, Internship
        Public Property Duration As Integer ' Duration in months
        Public Property SetaId As Guid?
        Public Property IsActive As Boolean
        Public Property CreatedAt As DateTime
        Public Property UpdatedAt As DateTime?
        
        ' Navigation properties
        Public Property Seta As Seta
        Public Property Contracts As List(Of Contract)
        
        Public Sub New()
            Id = Guid.NewGuid()
            CreatedAt = DateTime.UtcNow
            IsActive = True
            Contracts = New List(Of Contract)()
        End Sub
        
        ''' <summary>
        ''' Validates if the NQF level is within the valid range (1-10)
        ''' </summary>
        Public Function IsValidNQFLevel() As Boolean
            Return NQFLevel >= 1 AndAlso NQFLevel <= 10
        End Function
    End Class
End Namespace
