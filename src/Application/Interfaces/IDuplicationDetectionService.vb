Imports System
Imports System.Threading.Tasks
Imports IDP.Application.DTOs
Imports IDP.Domain.Entities

Namespace Interfaces
    Public Interface IDuplicationDetectionService
        Function CheckForDuplicatesAsync(learner As Learner) As Task(Of DuplicationCheckResultDto)
        Function CheckForDuplicatesByIdAsync(learnerId As Guid) As Task(Of DuplicationCheckResultDto)
        Function RunBulkDuplicationCheckAsync() As Task
    End Interface
End Namespace
