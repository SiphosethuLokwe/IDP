Imports System
Imports System.Threading.Tasks
Imports Microsoft.AspNetCore.Mvc
Imports Hangfire
Imports IDP.Application.Interfaces
Imports IDP.Application.DTOs
Imports IDP.Infrastructure.BackgroundJobs

Namespace Controllers
    <ApiController>
    <Route("api/[controller]")>
    Public Class DuplicationsController
        Inherits ControllerBase
        
        Private ReadOnly _duplicationService As IDuplicationDetectionService
        Private ReadOnly _flagRepository As IDuplicationFlagRepository
        Private ReadOnly _backgroundJobClient As IBackgroundJobClient
        
        Public Sub New(duplicationService As IDuplicationDetectionService, 
                      flagRepository As IDuplicationFlagRepository,
                      backgroundJobClient As IBackgroundJobClient)
            _duplicationService = duplicationService
            _flagRepository = flagRepository
            _backgroundJobClient = backgroundJobClient
        End Sub
        
        <HttpGet("check/{learnerId}")>
        Public Async Function CheckDuplicates(learnerId As Guid) As Task(Of ActionResult(Of DuplicationCheckResultDto))
            Try
                Dim result = Await _duplicationService.CheckForDuplicatesByIdAsync(learnerId)
                Return Ok(result)
            Catch ex As InvalidOperationException
                Return NotFound(New With {.message = ex.Message})
            Catch ex As Exception
                Return StatusCode(500, New With {.message = ex.Message})
            End Try
        End Function
        
        <HttpPost("run-bulk-check")>
        Public Function RunBulkCheck() As ActionResult
            Try
                Dim jobId = _backgroundJobClient.Enqueue(Of DuplicationCheckJob)(
                    Function(job) job.RunBulkDuplicationCheckAsync()
                )
                Return Ok(New With {.message = "Bulk duplication check started", .jobId = jobId})
            Catch ex As Exception
                Return StatusCode(500, New With {.message = ex.Message})
            End Try
        End Function
        
        <HttpGet("flags/pending")>
        Public Async Function GetPendingFlags() As Task(Of ActionResult(Of List(Of DuplicationFlagDto)))
            Try
                Dim flags = Await _flagRepository.GetPendingFlagsAsync()
                Dim dtos = flags.Select(Function(f) MapToDto(f)).ToList()
                Return Ok(dtos)
            Catch ex As Exception
                Return StatusCode(500, New With {.message = ex.Message})
            End Try
        End Function
        
        <HttpGet("flags/learner/{learnerId}")>
        Public Async Function GetFlagsByLearner(learnerId As Guid) As Task(Of ActionResult(Of List(Of DuplicationFlagDto)))
            Try
                Dim flags = Await _flagRepository.GetByLearnerIdAsync(learnerId)
                Dim dtos = flags.Select(Function(f) MapToDto(f)).ToList()
                Return Ok(dtos)
            Catch ex As Exception
                Return StatusCode(500, New With {.message = ex.Message})
            End Try
        End Function
        
        <HttpPut("flags/{flagId}/review")>
        Public Async Function ReviewFlag(flagId As Guid, <FromBody> dto As ReviewDuplicationDto) As Task(Of ActionResult)
            Try
                Dim flag = Await _flagRepository.GetByIdAsync(flagId)
                If flag Is Nothing Then
                    Return NotFound()
                End If
                
                flag.Status = dto.Status
                flag.ReviewedBy = dto.ReviewedBy
                flag.ReviewedAt = DateTime.UtcNow
                flag.Notes = dto.Notes
                
                If dto.Status = Domain.Enums.DuplicationStatus.Resolved Then
                    flag.ResolvedAt = DateTime.UtcNow
                End If
                
                Await _flagRepository.UpdateAsync(flag)
                Return Ok(New With {.message = "Flag reviewed successfully"})
            Catch ex As Exception
                Return StatusCode(500, New With {.message = ex.Message})
            End Try
        End Function
        
        Private Function MapToDto(flag As Domain.Entities.DuplicationFlag) As DuplicationFlagDto
            Return New DuplicationFlagDto() With {
                .Id = flag.Id,
                .LearnerId = flag.LearnerId,
                .DuplicateLearnerId = flag.DuplicateLearnerId,
                .MatchType = flag.MatchType,
                .ConfidenceScore = flag.ConfidenceScore,
                .MatchDetails = flag.MatchDetails,
                .Status = flag.Status,
                .CreatedAt = flag.CreatedAt,
                .ReviewedBy = flag.ReviewedBy,
                .ReviewedAt = flag.ReviewedAt,
                .Notes = flag.Notes,
                .LearnerName = If(flag.Learner IsNot Nothing, $"{flag.Learner.FirstName} {flag.Learner.LastName}", ""),
                .LearnerIdNumber = If(flag.Learner IsNot Nothing, flag.Learner.IdNumber, ""),
                .DuplicateLearnerName = If(flag.DuplicateLearner IsNot Nothing, $"{flag.DuplicateLearner.FirstName} {flag.DuplicateLearner.LastName}", ""),
                .DuplicateLearnerIdNumber = If(flag.DuplicateLearner IsNot Nothing, flag.DuplicateLearner.IdNumber, "")
            }
        End Function
    End Class
End Namespace
