Imports System
Imports System.Threading.Tasks
Imports IDP.Domain.Entities

Namespace Interfaces
    Public Interface ISetaRepository
        Function GetByIdAsync(id As Guid) As Task(Of Seta)
        Function GetBySetaCodeAsync(setaCode As String) As Task(Of Seta)
        Function GetAllAsync() As Task(Of List(Of Seta))
        Function GetActiveSetasAsync() As Task(Of List(Of Seta))
        Function AddAsync(seta As Seta) As Task(Of Seta)
        Function UpdateAsync(seta As Seta) As Task
        Function DeleteAsync(id As Guid) As Task
    End Interface
End Namespace
