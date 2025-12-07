Imports System
Imports System.Threading.Tasks
Imports IDP.Domain.Entities

Namespace Interfaces
    Public Interface IProgrammeRepository
        Function GetByIdAsync(id As Guid) As Task(Of Programme)
        Function GetByQualificationCodeAsync(qualificationCode As String) As Task(Of Programme)
        Function GetBySetaIdAsync(setaId As Guid) As Task(Of List(Of Programme))
        Function GetAllAsync() As Task(Of List(Of Programme))
        Function AddAsync(programme As Programme) As Task(Of Programme)
        Function UpdateAsync(programme As Programme) As Task
        Function DeleteAsync(id As Guid) As Task
    End Interface
End Namespace
