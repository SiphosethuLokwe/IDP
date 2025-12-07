Imports System
Imports System.Threading.Tasks
Imports Microsoft.EntityFrameworkCore
Imports IDP.Application.Interfaces
Imports IDP.Domain.Entities
Imports IDP.Domain.Enums

Namespace Persistence.Repositories
    Public Class DuplicationFlagRepository
        Implements IDuplicationFlagRepository
        
        Private ReadOnly _context As ApplicationDbContext
        
        Public Sub New(context As ApplicationDbContext)
            _context = context
        End Sub
        
        Public Async Function GetByIdAsync(id As Guid) As Task(Of DuplicationFlag) Implements IDuplicationFlagRepository.GetByIdAsync
            Return Await _context.DuplicationFlags _
                .Include(Function(f) f.Learner) _
                .Include(Function(f) f.DuplicateLearner) _
                .FirstOrDefaultAsync(Function(f) f.Id = id)
        End Function
        
        Public Async Function GetByLearnerIdAsync(learnerId As Guid) As Task(Of List(Of DuplicationFlag)) Implements IDuplicationFlagRepository.GetByLearnerIdAsync
            Return Await _context.DuplicationFlags _
                .Include(Function(f) f.DuplicateLearner) _
                .Where(Function(f) f.LearnerId = learnerId) _
                .OrderByDescending(Function(f) f.CreatedAt) _
                .ToListAsync()
        End Function
        
        Public Async Function GetPendingFlagsAsync() As Task(Of List(Of DuplicationFlag)) Implements IDuplicationFlagRepository.GetPendingFlagsAsync
            Return Await GetByStatusAsync(DuplicationStatus.Pending)
        End Function
        
        Public Async Function GetByStatusAsync(status As DuplicationStatus) As Task(Of List(Of DuplicationFlag)) Implements IDuplicationFlagRepository.GetByStatusAsync
            Return Await _context.DuplicationFlags _
                .Include(Function(f) f.Learner) _
                .Include(Function(f) f.DuplicateLearner) _
                .Where(Function(f) f.Status = status) _
                .OrderByDescending(Function(f) f.CreatedAt) _
                .ToListAsync()
        End Function
        
        Public Async Function AddAsync(flag As DuplicationFlag) As Task(Of DuplicationFlag) Implements IDuplicationFlagRepository.AddAsync
            _context.DuplicationFlags.Add(flag)
            Await _context.SaveChangesAsync()
            Return flag
        End Function
        
        Public Async Function UpdateAsync(flag As DuplicationFlag) As Task(Of DuplicationFlag) Implements IDuplicationFlagRepository.UpdateAsync
            _context.DuplicationFlags.Update(flag)
            Await _context.SaveChangesAsync()
            Return flag
        End Function
        
        Public Async Function BulkAddAsync(flags As List(Of DuplicationFlag)) As Task Implements IDuplicationFlagRepository.BulkAddAsync
            _context.DuplicationFlags.AddRange(flags)
            Await _context.SaveChangesAsync()
        End Function
    End Class
End Namespace
