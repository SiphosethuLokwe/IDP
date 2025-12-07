Imports System
Imports System.Threading.Tasks
Imports IDP.Domain.Entities
Imports IDP.Domain.Enums

Namespace Interfaces
    Public Interface IDuplicationFlagRepository
        Function GetByIdAsync(id As Guid) As Task(Of DuplicationFlag)
        Function GetByLearnerIdAsync(learnerId As Guid) As Task(Of List(Of DuplicationFlag))
        Function GetPendingFlagsAsync() As Task(Of List(Of DuplicationFlag))
        Function GetByStatusAsync(status As DuplicationStatus) As Task(Of List(Of DuplicationFlag))
        Function AddAsync(flag As DuplicationFlag) As Task(Of DuplicationFlag)
        Function UpdateAsync(flag As DuplicationFlag) As Task(Of DuplicationFlag)
        Function BulkAddAsync(flags As List(Of DuplicationFlag)) As Task
    End Interface
End Namespace
