Imports System
Imports System.Threading.Tasks
Imports Microsoft.EntityFrameworkCore
Imports IDP.Application.Interfaces
Imports IDP.Domain.Entities
Imports IDP.Infrastructure.Persistence

Namespace Persistence.Repositories
    Public Class SetaRepository
        Implements ISetaRepository
        
        Private ReadOnly _context As ApplicationDbContext
        
        Public Sub New(context As ApplicationDbContext)
            _context = context
        End Sub
        
        Public Async Function GetByIdAsync(id As Guid) As Task(Of Seta) Implements ISetaRepository.GetByIdAsync
            Return Await _context.Setas.FindAsync(id)
        End Function
        
        Public Async Function GetBySetaCodeAsync(setaCode As String) As Task(Of Seta) Implements ISetaRepository.GetBySetaCodeAsync
            Return Await _context.Setas _
                .FirstOrDefaultAsync(Function(s) s.SetaCode = setaCode)
        End Function
        
        Public Async Function GetAllAsync() As Task(Of List(Of Seta)) Implements ISetaRepository.GetAllAsync
            Return Await _context.Setas.ToListAsync()
        End Function
        
        Public Async Function GetActiveSetasAsync() As Task(Of List(Of Seta)) Implements ISetaRepository.GetActiveSetasAsync
            Return Await _context.Setas _
                .Where(Function(s) s.IsActive) _
                .OrderBy(Function(s) s.Name) _
                .ToListAsync()
        End Function
        
        Public Async Function AddAsync(seta As Seta) As Task(Of Seta) Implements ISetaRepository.AddAsync
            _context.Setas.Add(seta)
            Await _context.SaveChangesAsync()
            Return seta
        End Function
        
        Public Async Function UpdateAsync(seta As Seta) As Task Implements ISetaRepository.UpdateAsync
            seta.UpdatedAt = DateTime.UtcNow
            _context.Setas.Update(seta)
            Await _context.SaveChangesAsync()
        End Function
        
        Public Async Function DeleteAsync(id As Guid) As Task Implements ISetaRepository.DeleteAsync
            Dim seta = Await GetByIdAsync(id)
            If seta IsNot Nothing Then
                _context.Setas.Remove(seta)
                Await _context.SaveChangesAsync()
            End If
        End Function
    End Class
End Namespace
