Imports System
Imports System.Threading.Tasks
Imports Microsoft.EntityFrameworkCore
Imports IDP.Application.Interfaces
Imports IDP.Domain.Entities

Namespace Persistence.Repositories
    Public Class LearnerRepository
        Implements ILearnerRepository
        
        Private ReadOnly _context As ApplicationDbContext
        
        Public Sub New(context As ApplicationDbContext)
            _context = context
        End Sub
        
        Public Async Function GetByIdAsync(id As Guid) As Task(Of Learner) Implements ILearnerRepository.GetByIdAsync
            Return Await _context.Learners _
                .Include(Function(l) l.DuplicationFlags) _
                .FirstOrDefaultAsync(Function(l) l.Id = id)
        End Function
        
        Public Async Function GetByIdNumberAsync(idNumber As String) As Task(Of Learner) Implements ILearnerRepository.GetByIdNumberAsync
            Return Await _context.Learners _
                .Include(Function(l) l.DuplicationFlags) _
                .FirstOrDefaultAsync(Function(l) l.IdNumber = idNumber)
        End Function
        
        Public Async Function GetAllAsync() As Task(Of List(Of Learner)) Implements ILearnerRepository.GetAllAsync
            Return Await _context.Learners _
                .Where(Function(l) l.IsActive) _
                .ToListAsync()
        End Function
        
        Public Async Function GetBySetaCodeAsync(setaCode As String) As Task(Of List(Of Learner)) Implements ILearnerRepository.GetBySetaCodeAsync
            Return Await _context.Learners _
                .Where(Function(l) l.SetaCode = setaCode AndAlso l.IsActive) _
                .ToListAsync()
        End Function
        
        Public Async Function AddAsync(learner As Learner) As Task(Of Learner) Implements ILearnerRepository.AddAsync
            _context.Learners.Add(learner)
            Await _context.SaveChangesAsync()
            Return learner
        End Function
        
        Public Async Function UpdateAsync(learner As Learner) As Task(Of Learner) Implements ILearnerRepository.UpdateAsync
            _context.Learners.Update(learner)
            Await _context.SaveChangesAsync()
            Return learner
        End Function
        
        Public Async Function DeleteAsync(id As Guid) As Task(Of Boolean) Implements ILearnerRepository.DeleteAsync
            Dim learner = Await _context.Learners.FindAsync(id)
            If learner Is Nothing Then
                Return False
            End If
            
            learner.IsActive = False
            learner.UpdatedAt = DateTime.UtcNow
            Await _context.SaveChangesAsync()
            Return True
        End Function
        
        Public Async Function SearchPotentialDuplicatesAsync(learner As Learner) As Task(Of List(Of Learner)) Implements ILearnerRepository.SearchPotentialDuplicatesAsync
            ' Search for potential duplicates based on various criteria
            Dim query = _context.Learners.Where(Function(l) l.IsActive AndAlso l.Id <> learner.Id)
            
            ' Build a complex query to find potential matches
            Dim potentialDuplicates = Await query _
                .Where(Function(l) 
                    (Not String.IsNullOrEmpty(learner.IdNumber) AndAlso l.IdNumber = learner.IdNumber) OrElse
                    (Not String.IsNullOrEmpty(learner.PassportNumber) AndAlso l.PassportNumber = learner.PassportNumber) OrElse
                    (Not String.IsNullOrEmpty(learner.PhoneNumber) AndAlso l.PhoneNumber = learner.PhoneNumber) OrElse
                    (Not String.IsNullOrEmpty(learner.Email) AndAlso l.Email = learner.Email) OrElse
                    (l.FirstName = learner.FirstName AndAlso l.LastName = learner.LastName AndAlso 
                     learner.DateOfBirth.HasValue AndAlso l.DateOfBirth.HasValue AndAlso 
                     l.DateOfBirth.Value = learner.DateOfBirth.Value)
                End Function) _
                .ToListAsync()
            
            Return potentialDuplicates
        End Function
    End Class
End Namespace
