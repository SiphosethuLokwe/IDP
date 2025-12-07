Imports System
Imports System.Threading.Tasks
Imports Microsoft.EntityFrameworkCore
Imports IDP.Application.Interfaces
Imports IDP.Domain.Entities
Imports IDP.Infrastructure.Persistence

Namespace Persistence.Repositories
    Public Class ProgrammeRepository
        Implements IProgrammeRepository
        
        Private ReadOnly _context As ApplicationDbContext
        
        Public Sub New(context As ApplicationDbContext)
            _context = context
        End Sub
        
        Public Async Function GetByIdAsync(id As Guid) As Task(Of Programme) Implements IProgrammeRepository.GetByIdAsync
            Return Await _context.Programmes _
                .Include(Function(p) p.Seta) _
                .FirstOrDefaultAsync(Function(p) p.Id = id)
        End Function
        
        Public Async Function GetByQualificationCodeAsync(qualificationCode As String) As Task(Of Programme) Implements IProgrammeRepository.GetByQualificationCodeAsync
            Return Await _context.Programmes _
                .Include(Function(p) p.Seta) _
                .FirstOrDefaultAsync(Function(p) p.QualificationCode = qualificationCode)
        End Function
        
        Public Async Function GetBySetaIdAsync(setaId As Guid) As Task(Of List(Of Programme)) Implements IProgrammeRepository.GetBySetaIdAsync
            Dim allProgrammes = Await _context.Programmes _
                .Include(Function(p) p.Seta) _
                .Where(Function(p) p.SetaId.HasValue AndAlso p.SetaId.Value = setaId) _
                .OrderBy(Function(p) p.Title) _
                .ToListAsync()
            Return allProgrammes.Where(Function(p) p.IsActive = True).ToList()
        End Function
        
        Public Async Function GetAllAsync() As Task(Of List(Of Programme)) Implements IProgrammeRepository.GetAllAsync
            Dim allProgrammes = Await _context.Programmes _
                .Include(Function(p) p.Seta) _
                .ToListAsync()
            Return allProgrammes.Where(Function(p) p.IsActive = True).ToList()
        End Function
        
        Public Async Function AddAsync(programme As Programme) As Task(Of Programme) Implements IProgrammeRepository.AddAsync
            _context.Programmes.Add(programme)
            Await _context.SaveChangesAsync()
            Return programme
        End Function
        
        Public Async Function UpdateAsync(programme As Programme) As Task Implements IProgrammeRepository.UpdateAsync
            programme.UpdatedAt = DateTime.UtcNow
            _context.Programmes.Update(programme)
            Await _context.SaveChangesAsync()
        End Function
        
        Public Async Function DeleteAsync(id As Guid) As Task Implements IProgrammeRepository.DeleteAsync
            Dim programme = Await _context.Programmes.FindAsync(id)
            If programme IsNot Nothing Then
                programme.IsActive = False
                Await UpdateAsync(programme)
            End If
        End Function
    End Class
End Namespace
