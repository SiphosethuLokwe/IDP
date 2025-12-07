Imports System
Imports System.Threading.Tasks
Imports IDP.Domain.Entities
Imports IDP.Domain.Enums

Namespace Interfaces
    Public Interface IContractRepository
        Function GetByIdAsync(id As Guid) As Task(Of Contract)
        Function GetByContractNumberAsync(contractNumber As String) As Task(Of Contract)
        Function GetByLearnerIdAsync(learnerId As Guid) As Task(Of List(Of Contract))
        Function GetActiveContractsByLearnerIdAsync(learnerId As Guid) As Task(Of List(Of Contract))
        Function GetBySetaIdAsync(setaId As Guid) As Task(Of List(Of Contract))
        Function GetByProgrammeIdAsync(programmeId As Guid) As Task(Of List(Of Contract))
        
        ''' <summary>
        ''' Checks if learner has an active contract for a specific programme
        ''' </summary>
        Function HasActiveContractForProgrammeAsync(learnerId As Guid, programmeId As Guid) As Task(Of Boolean)
        
        ''' <summary>
        ''' Gets active contracts for learner in a specific SETA
        ''' </summary>
        Function GetActiveContractsByLearnerAndSetaAsync(learnerId As Guid, setaId As Guid) As Task(Of List(Of Contract))
        
        ''' <summary>
        ''' Checks for overlapping contracts
        ''' </summary>
        Function HasOverlappingContractsAsync(learnerId As Guid, startDate As Date, endDate As Date) As Task(Of List(Of Contract))
        
        Function AddAsync(contract As Contract) As Task(Of Contract)
        Function UpdateAsync(contract As Contract) As Task
        Function DeleteAsync(id As Guid) As Task
    End Interface
End Namespace
