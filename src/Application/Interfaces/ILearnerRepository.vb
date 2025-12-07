Imports System
Imports System.Threading.Tasks
Imports IDP.Domain.Entities

Namespace Interfaces
    Public Interface ILearnerRepository
        Function GetByIdAsync(id As Guid) As Task(Of Learner)
        Function GetByIdNumberAsync(idNumber As String) As Task(Of Learner)
        Function GetAllAsync() As Task(Of List(Of Learner))
        Function GetBySetaCodeAsync(setaCode As String) As Task(Of List(Of Learner))
        Function AddAsync(learner As Learner) As Task(Of Learner)
        Function UpdateAsync(learner As Learner) As Task(Of Learner)
        Function DeleteAsync(id As Guid) As Task(Of Boolean)
        Function SearchPotentialDuplicatesAsync(learner As Learner) As Task(Of List(Of Learner))
    End Interface
End Namespace
