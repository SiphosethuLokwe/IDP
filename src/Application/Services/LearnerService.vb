Imports System
Imports System.Threading.Tasks
Imports IDP.Application.DTOs
Imports IDP.Application.Interfaces
Imports IDP.Domain.Entities

Namespace Services
    Public Class LearnerService
        Private ReadOnly _learnerRepository As ILearnerRepository
        Private ReadOnly _duplicationService As IDuplicationDetectionService
        
        Public Sub New(learnerRepository As ILearnerRepository, duplicationService As IDuplicationDetectionService)
            _learnerRepository = learnerRepository
            _duplicationService = duplicationService
        End Sub
        
        Public Async Function GetByIdAsync(id As Guid) As Task(Of LearnerDto)
            Dim learner = Await _learnerRepository.GetByIdAsync(id)
            Return MapToDto(learner)
        End Function
        
        Public Async Function GetAllAsync() As Task(Of List(Of LearnerDto))
            Dim learners = Await _learnerRepository.GetAllAsync()
            Return learners.Select(Function(l) MapToDto(l)).ToList()
        End Function
        
        Public Async Function CreateAsync(dto As CreateLearnerDto) As Task(Of LearnerDto)
            ' Check if learner already exists
            Dim existing = Await _learnerRepository.GetByIdNumberAsync(dto.IdNumber)
            If existing IsNot Nothing Then
                Throw New InvalidOperationException("Learner with this ID number already exists")
            End If
            
            Dim learner = New Learner() With {
                .IdNumber = dto.IdNumber,
                .FirstName = dto.FirstName,
                .LastName = dto.LastName,
                .DateOfBirth = dto.DateOfBirth,
                .PhoneNumber = dto.PhoneNumber,
                .Email = dto.Email,
                .AlternativeIdNumber = dto.AlternativeIdNumber,
                .PassportNumber = dto.PassportNumber,
                .SetaCode = dto.SetaCode,
                .OrganizationId = dto.OrganizationId
            }
            
            Dim created = Await _learnerRepository.AddAsync(learner)
            
            ' Trigger duplicate check asynchronously (fire and forget for now)
            Dim duplicateCheckTask = _duplicationService.CheckForDuplicatesAsync(created)
            
            Return MapToDto(created)
        End Function
        
        Public Async Function UpdateAsync(id As Guid, dto As UpdateLearnerDto) As Task(Of LearnerDto)
            Dim learner = Await _learnerRepository.GetByIdAsync(id)
            If learner Is Nothing Then
                Throw New InvalidOperationException("Learner not found")
            End If
            
            learner.FirstName = dto.FirstName
            learner.LastName = dto.LastName
            learner.PhoneNumber = dto.PhoneNumber
            learner.Email = dto.Email
            learner.AlternativeIdNumber = dto.AlternativeIdNumber
            learner.PassportNumber = dto.PassportNumber
            learner.IsActive = dto.IsActive
            learner.UpdatedAt = DateTime.UtcNow
            
            Dim updated = Await _learnerRepository.UpdateAsync(learner)
            Return MapToDto(updated)
        End Function
        
        Public Async Function DeleteAsync(id As Guid) As Task(Of Boolean)
            Return Await _learnerRepository.DeleteAsync(id)
        End Function
        
        Private Function MapToDto(learner As Learner) As LearnerDto
            If learner Is Nothing Then Return Nothing
            
            Return New LearnerDto() With {
                .Id = learner.Id,
                .IdNumber = learner.IdNumber,
                .FirstName = learner.FirstName,
                .LastName = learner.LastName,
                .DateOfBirth = learner.DateOfBirth,
                .PhoneNumber = learner.PhoneNumber,
                .Email = learner.Email,
                .AlternativeIdNumber = learner.AlternativeIdNumber,
                .PassportNumber = learner.PassportNumber,
                .SetaCode = learner.SetaCode,
                .OrganizationId = learner.OrganizationId,
                .IsActive = learner.IsActive
            }
        End Function
    End Class
End Namespace
