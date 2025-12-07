Imports System
Imports IDP.Domain.Enums

Namespace Entities
    ''' <summary>
    ''' Represents a learnership/training contract between a Learner and a SETA for a specific Programme.
    ''' This is the core entity for tracking active funding and preventing duplicate funding fraud.
    ''' </summary>
    Public Class Contract
        Public Property Id As Guid
        Public Property ContractNumber As String ' Unique contract reference
        Public Property LearnerId As Guid
        Public Property SetaId As Guid
        Public Property ProgrammeId As Guid
        Public Property ProviderId As String ' Training provider/employer
        Public Property ProviderName As String
        Public Property Status As ContractStatus
        Public Property StartDate As Date
        Public Property EndDate As Date
        Public Property ActualEndDate As Date?
        Public Property FundingAmount As Decimal?
        Public Property IsActive As Boolean
        Public Property Notes As String
        Public Property CreatedAt As DateTime
        Public Property UpdatedAt As DateTime?
        Public Property CreatedBy As String
        Public Property UpdatedBy As String
        
        ' Navigation properties
        Public Property Learner As Learner
        Public Property Seta As Seta
        Public Property Programme As Programme
        
        Public Sub New()
            Id = Guid.NewGuid()
            CreatedAt = DateTime.UtcNow
            IsActive = True
            Status = ContractStatus.Pending
        End Sub
        
        ''' <summary>
        ''' Checks if the contract is currently active and receiving funding
        ''' </summary>
        Public Function IsCurrentlyActive() As Boolean
            Return Status = ContractStatus.Active AndAlso 
                   DateTime.Now >= StartDate AndAlso 
                   DateTime.Now <= EndDate AndAlso 
                   IsActive
        End Function
        
        ''' <summary>
        ''' Checks if the contract is closed (completed, terminated, cancelled, or expired)
        ''' </summary>
        Public Function IsClosed() As Boolean
            Return Status = ContractStatus.Completed OrElse 
                   Status = ContractStatus.Terminated OrElse 
                   Status = ContractStatus.Cancelled OrElse 
                   Status = ContractStatus.Expired
        End Function
        
        ''' <summary>
        ''' Gets the remaining days in the contract
        ''' </summary>
        Public Function GetRemainingDays() As Integer
            If Not IsCurrentlyActive() Then
                Return 0
            End If
            
            Dim remainingDays = (EndDate - DateTime.Now).Days
            Return Math.Max(0, remainingDays)
        End Function
        
        ''' <summary>
        ''' Checks if contract overlaps with another contract period
        ''' </summary>
        Public Function OverlapsWith(otherContract As Contract) As Boolean
            Return StartDate <= otherContract.EndDate AndAlso EndDate >= otherContract.StartDate
        End Function
    End Class
End Namespace
