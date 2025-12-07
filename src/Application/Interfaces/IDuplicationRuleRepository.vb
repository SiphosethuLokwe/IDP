Imports System
Imports System.Threading.Tasks
Imports IDP.Domain.Entities

Namespace Interfaces
    Public Interface IDuplicationRuleRepository
        Function GetAllActiveRulesAsync() As Task(Of List(Of DuplicationRule))
        Function GetByIdAsync(id As Guid) As Task(Of DuplicationRule)
        Function AddAsync(rule As DuplicationRule) As Task(Of DuplicationRule)
        Function UpdateAsync(rule As DuplicationRule) As Task(Of DuplicationRule)
        Function DeleteAsync(id As Guid) As Task(Of Boolean)
    End Interface
End Namespace
