Imports System
Imports System.Threading.Tasks
Imports Microsoft.EntityFrameworkCore
Imports IDP.Application.Interfaces
Imports IDP.Domain.Entities
Imports IDP.Domain.Enums
Imports IDP.Infrastructure.Persistence

Namespace Persistence.Repositories
    Public Class ContractRepository
        Implements IContractRepository
        
        Private ReadOnly _context As ApplicationDbContext
        
        Public Sub New(context As ApplicationDbContext)
            _context = context
        End Sub
        
        Public Async Function GetByIdAsync(id As Guid) As Task(Of Contract) Implements IContractRepository.GetByIdAsync
            Return Await _context.Contracts _
                .Include(Function(c) c.Learner) _
                .Include(Function(c) c.Seta) _
                .Include(Function(c) c.Programme) _
                .FirstOrDefaultAsync(Function(c) c.Id = id)
        End Function
        
        Public Async Function GetByContractNumberAsync(contractNumber As String) As Task(Of Contract) Implements IContractRepository.GetByContractNumberAsync
            Return Await _context.Contracts _
                .Include(Function(c) c.Learner) _
                .Include(Function(c) c.Seta) _
                .Include(Function(c) c.Programme) _
                .FirstOrDefaultAsync(Function(c) c.ContractNumber = contractNumber)
        End Function
        
        Public Async Function GetByLearnerIdAsync(learnerId As Guid) As Task(Of List(Of Contract)) Implements IContractRepository.GetByLearnerIdAsync
            Return Await _context.Contracts _
                .Include(Function(c) c.Seta) _
                .Include(Function(c) c.Programme) _
                .Where(Function(c) c.LearnerId = learnerId) _
                .OrderByDescending(Function(c) c.StartDate) _
                .ToListAsync()
        End Function
        
        Public Async Function GetActiveContractsByLearnerIdAsync(learnerId As Guid) As Task(Of List(Of Contract)) Implements IContractRepository.GetActiveContractsByLearnerIdAsync
            Dim today = DateTime.Today
            Return Await _context.Contracts _
                .Include(Function(c) c.Seta) _
                .Include(Function(c) c.Programme) _
                .Where(Function(c) c.LearnerId = learnerId AndAlso 
                                   c.Status = ContractStatus.Active AndAlso
                                   c.IsActive AndAlso
                                   c.StartDate <= today AndAlso
                                   c.EndDate >= today) _
                .ToListAsync()
        End Function
        
        Public Async Function GetBySetaIdAsync(setaId As Guid) As Task(Of List(Of Contract)) Implements IContractRepository.GetBySetaIdAsync
            Return Await _context.Contracts _
                .Include(Function(c) c.Learner) _
                .Include(Function(c) c.Programme) _
                .Where(Function(c) c.SetaId = setaId) _
                .ToListAsync()
        End Function
        
        Public Async Function GetByProgrammeIdAsync(programmeId As Guid) As Task(Of List(Of Contract)) Implements IContractRepository.GetByProgrammeIdAsync
            Return Await _context.Contracts _
                .Include(Function(c) c.Learner) _
                .Include(Function(c) c.Seta) _
                .Where(Function(c) c.ProgrammeId = programmeId) _
                .ToListAsync()
        End Function
        
        Public Async Function HasActiveContractForProgrammeAsync(learnerId As Guid, programmeId As Guid) As Task(Of Boolean) Implements IContractRepository.HasActiveContractForProgrammeAsync
            Dim today = DateTime.Today
            Return Await _context.Contracts _
                .AnyAsync(Function(c) c.LearnerId = learnerId AndAlso
                                     c.ProgrammeId = programmeId AndAlso
                                     c.Status = ContractStatus.Active AndAlso
                                     c.IsActive AndAlso
                                     c.StartDate <= today AndAlso
                                     c.EndDate >= today)
        End Function
        
        Public Async Function GetActiveContractsByLearnerAndSetaAsync(learnerId As Guid, setaId As Guid) As Task(Of List(Of Contract)) Implements IContractRepository.GetActiveContractsByLearnerAndSetaAsync
            Dim today = DateTime.Today
            Return Await _context.Contracts _
                .Include(Function(c) c.Programme) _
                .Where(Function(c) c.LearnerId = learnerId AndAlso
                                   c.SetaId = setaId AndAlso
                                   c.Status = ContractStatus.Active AndAlso
                                   c.IsActive AndAlso
                                   c.StartDate <= today AndAlso
                                   c.EndDate >= today) _
                .ToListAsync()
        End Function
        
        Public Async Function HasOverlappingContractsAsync(learnerId As Guid, startDate As Date, endDate As Date) As Task(Of List(Of Contract)) Implements IContractRepository.HasOverlappingContractsAsync
            Return Await _context.Contracts _
                .Include(Function(c) c.Seta) _
                .Include(Function(c) c.Programme) _
                .Where(Function(c) c.LearnerId = learnerId AndAlso
                                   c.IsActive AndAlso
                                   c.StartDate <= endDate AndAlso
                                   c.EndDate >= startDate) _
                .ToListAsync()
        End Function
        
        Public Async Function AddAsync(contract As Contract) As Task(Of Contract) Implements IContractRepository.AddAsync
            _context.Contracts.Add(contract)
            Await _context.SaveChangesAsync()
            Return contract
        End Function
        
        Public Async Function UpdateAsync(contract As Contract) As Task Implements IContractRepository.UpdateAsync
            contract.UpdatedAt = DateTime.UtcNow
            _context.Contracts.Update(contract)
            Await _context.SaveChangesAsync()
        End Function
        
        Public Async Function DeleteAsync(id As Guid) As Task Implements IContractRepository.DeleteAsync
            Dim contract = Await _context.Contracts.FindAsync(id)
            If contract IsNot Nothing Then
                contract.IsActive = False
                Await UpdateAsync(contract)
            End If
        End Function
    End Class
End Namespace
