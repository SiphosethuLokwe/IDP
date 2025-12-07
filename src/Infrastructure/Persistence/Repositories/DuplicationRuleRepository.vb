Imports System
Imports System.Threading.Tasks
Imports Microsoft.EntityFrameworkCore
Imports IDP.Application.Interfaces
Imports IDP.Domain.Entities

Namespace Persistence.Repositories
    Public Class DuplicationRuleRepository
        Implements IDuplicationRuleRepository
        
        Private ReadOnly _context As ApplicationDbContext
        
        Public Sub New(context As ApplicationDbContext)
            _context = context
        End Sub
        
        Public Async Function GetAllActiveRulesAsync() As Task(Of List(Of DuplicationRule)) Implements IDuplicationRuleRepository.GetAllActiveRulesAsync
            Return Await _context.DuplicationRules _
                .Where(Function(r) r.IsActive) _
                .OrderBy(Function(r) r.Priority) _
                .ToListAsync()
        End Function
        
        Public Async Function GetByIdAsync(id As Guid) As Task(Of DuplicationRule) Implements IDuplicationRuleRepository.GetByIdAsync
            Return Await _context.DuplicationRules.FindAsync(id)
        End Function
        
        Public Async Function AddAsync(rule As DuplicationRule) As Task(Of DuplicationRule) Implements IDuplicationRuleRepository.AddAsync
            _context.DuplicationRules.Add(rule)
            Await _context.SaveChangesAsync()
            Return rule
        End Function
        
        Public Async Function UpdateAsync(rule As DuplicationRule) As Task(Of DuplicationRule) Implements IDuplicationRuleRepository.UpdateAsync
            rule.UpdatedAt = DateTime.UtcNow
            _context.DuplicationRules.Update(rule)
            Await _context.SaveChangesAsync()
            Return rule
        End Function
        
        Public Async Function DeleteAsync(id As Guid) As Task(Of Boolean) Implements IDuplicationRuleRepository.DeleteAsync
            Dim rule = Await _context.DuplicationRules.FindAsync(id)
            If rule Is Nothing Then
                Return False
            End If
            
            rule.IsActive = False
            rule.UpdatedAt = DateTime.UtcNow
            Await _context.SaveChangesAsync()
            Return True
        End Function
    End Class
End Namespace
