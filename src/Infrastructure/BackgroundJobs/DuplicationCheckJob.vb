Imports System
Imports System.Threading.Tasks
Imports Hangfire
Imports IDP.Application.Interfaces

Namespace BackgroundJobs
    Public Class DuplicationCheckJob
        Private ReadOnly _duplicationService As IDuplicationDetectionService
        Private ReadOnly _learnerRepository As ILearnerRepository
        
        Public Sub New(duplicationService As IDuplicationDetectionService, learnerRepository As ILearnerRepository)
            _duplicationService = duplicationService
            _learnerRepository = learnerRepository
        End Sub
        
        <AutomaticRetry(Attempts:=3)>
        Public Async Function RunBulkDuplicationCheckAsync() As Task
            Console.WriteLine($"Starting bulk duplication check at {DateTime.UtcNow}")
            Await _duplicationService.RunBulkDuplicationCheckAsync()
            Console.WriteLine($"Completed bulk duplication check at {DateTime.UtcNow}")
        End Function
        
        <AutomaticRetry(Attempts:=3)>
        Public Async Function CheckLearnerDuplicatesAsync(learnerId As Guid) As Task
            Console.WriteLine($"Checking duplicates for learner {learnerId}")
            Await _duplicationService.CheckForDuplicatesByIdAsync(learnerId)
            Console.WriteLine($"Completed duplicate check for learner {learnerId}")
        End Function
        
        Public Shared Sub ScheduleRecurringJobs()
            ' Run bulk duplication check daily at midnight
            RecurringJob.AddOrUpdate(Of DuplicationCheckJob)(
                "daily-duplication-check",
                Function(job) job.RunBulkDuplicationCheckAsync(),
                Cron.Daily()
            )
        End Sub
    End Class
End Namespace
