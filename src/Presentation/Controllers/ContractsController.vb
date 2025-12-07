Imports Microsoft.AspNetCore.Mvc
Imports IDP.Application.Interfaces
Imports IDP.Application.Services
Imports IDP.Domain.Entities
Imports System.Threading.Tasks

Namespace Controllers
    <ApiController>
    <Route("api/[controller]")>
    Public Class ContractsController
        Inherits ControllerBase
        
        Private ReadOnly _contractRepository As IContractRepository
        Private ReadOnly _crossSetaValidation As CrossSetaValidationService
        
        Public Sub New(contractRepository As IContractRepository,
                      crossSetaValidation As CrossSetaValidationService)
            _contractRepository = contractRepository
            _crossSetaValidation = crossSetaValidation
        End Sub
        
        ''' <summary>
        ''' Get contract by ID
        ''' </summary>
        <HttpGet("{id}")>
        Public Async Function GetById(id As Guid) As Task(Of ActionResult(Of Contract))
            Dim contract = Await _contractRepository.GetByIdAsync(id)
            
            If contract Is Nothing Then
                Return NotFound()
            End If
            
            Return Ok(contract)
        End Function
        
        ''' <summary>
        ''' Get all contracts for a learner
        ''' </summary>
        <HttpGet("learner/{learnerId}")>
        Public Async Function GetByLearnerId(learnerId As Guid) As Task(Of ActionResult(Of List(Of Contract)))
            Dim contracts = Await _contractRepository.GetByLearnerIdAsync(learnerId)
            Return Ok(contracts)
        End Function
        
        ''' <summary>
        ''' Get active contracts for a learner
        ''' </summary>
        <HttpGet("learner/{learnerId}/active")>
        Public Async Function GetActiveByLearnerId(learnerId As Guid) As Task(Of ActionResult(Of List(Of Contract)))
            Dim contracts = Await _contractRepository.GetActiveContractsByLearnerIdAsync(learnerId)
            Return Ok(contracts)
        End Function
        
        ''' <summary>
        ''' Get learner's complete training history across all SETAs
        ''' </summary>
        <HttpGet("learner/{learnerId}/history")>
        Public Async Function GetLearnerHistory(learnerId As Guid) As Task(Of ActionResult(Of LearnerSetaHistory))
            Dim history = Await _crossSetaValidation.GetLearnerHistoryAcrossSetasAsync(learnerId)
            Return Ok(history)
        End Function
        
        ''' <summary>
        ''' Validate if a new contract can be created (cross-SETA validation)
        ''' </summary>
        <HttpPost("validate")>
        Public Async Function ValidateNewContract(<FromBody> request As ContractValidationRequest) As Task(Of ActionResult(Of CrossSetaValidationResult))
            Try
                Dim result = Await _crossSetaValidation.ValidateNewRegistrationAsync(
                    request.LearnerId,
                    request.SetaId,
                    request.ProgrammeId
                )
                
                Return Ok(result)
            Catch ex As Exception
                Return BadRequest($"Validation error: {ex.Message}")
            End Try
        End Function
        
        ''' <summary>
        ''' Detect potential fraud for a learner
        ''' </summary>
        <HttpGet("learner/{learnerId}/fraud-check")>
        Public Async Function CheckForFraud(learnerId As Guid) As Task(Of ActionResult(Of List(Of FraudAlert)))
            Dim alerts = Await _crossSetaValidation.DetectCrossSetaFraudAsync(learnerId)
            Return Ok(alerts)
        End Function
        
        ''' <summary>
        ''' Create new contract with validation
        ''' </summary>
        <HttpPost>
        Public Async Function Create(<FromBody> contract As Contract) As Task(Of ActionResult(Of Contract))
            Try
                ' Validate before creating
                Dim validation = Await _crossSetaValidation.ValidateNewRegistrationAsync(
                    contract.LearnerId,
                    contract.SetaId,
                    contract.ProgrammeId
                )
                
                If Not validation.IsAllowed Then
                    Return BadRequest(New With {
                        .error = "Contract not allowed",
                        .reason = validation.Reason,
                        .validation = validation
                    })
                End If
                
                ' Create contract
                Dim createdContract = Await _contractRepository.AddAsync(contract)
                
                Return CreatedAtAction(NameOf(GetById), New With {.id = createdContract.Id}, createdContract)
            Catch ex As Exception
                Return BadRequest($"Error creating contract: {ex.Message}")
            End Try
        End Function
        
        ''' <summary>
        ''' Update contract
        ''' </summary>
        <HttpPut("{id}")>
        Public Async Function Update(id As Guid, <FromBody> contract As Contract) As Task(Of ActionResult)
            If id <> contract.Id Then
                Return BadRequest("ID mismatch")
            End If
            
            Try
                Await _contractRepository.UpdateAsync(contract)
                Return NoContent()
            Catch ex As Exception
                Return BadRequest($"Error updating contract: {ex.Message}")
            End Try
        End Function
        
        ''' <summary>
        ''' Check for overlapping contracts
        ''' </summary>
        <HttpPost("check-overlap")>
        Public Async Function CheckOverlap(<FromBody> request As OverlapCheckRequest) As Task(Of ActionResult(Of List(Of Contract)))
            Dim overlapping = Await _contractRepository.HasOverlappingContractsAsync(
                request.LearnerId,
                request.StartDate,
                request.EndDate
            )
            
            Return Ok(overlapping)
        End Function
    End Class
    
    Public Class ContractValidationRequest
        Public Property LearnerId As Guid
        Public Property SetaId As Guid
        Public Property ProgrammeId As Guid
    End Class
    
    Public Class OverlapCheckRequest
        Public Property LearnerId As Guid
        Public Property StartDate As Date
        Public Property EndDate As Date
    End Class
End Namespace
