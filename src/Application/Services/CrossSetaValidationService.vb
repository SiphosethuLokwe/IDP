Imports System
Imports System.Threading.Tasks
Imports IDP.Application.Interfaces
Imports IDP.Domain.Entities
Imports IDP.Domain.Enums

Namespace Services
    ''' <summary>
    ''' SETA-aware validation service implementing the smart duplicate detection rules.
    ''' Handles cross-SETA duplicate detection based on active contracts and funding status.
    ''' </summary>
    Public Class CrossSetaValidationService
        Private ReadOnly _contractRepository As IContractRepository
        Private ReadOnly _learnerRepository As ILearnerRepository
        Private ReadOnly _programmeRepository As IProgrammeRepository
        
        Public Sub New(contractRepository As IContractRepository,
                      learnerRepository As ILearnerRepository,
                      programmeRepository As IProgrammeRepository)
            _contractRepository = contractRepository
            _learnerRepository = learnerRepository
            _programmeRepository = programmeRepository
        End Sub
        
        ''' <summary>
        ''' Validates if a learner can be registered for a new contract based on SETA rules.
        ''' Implements the rules from specification section 10.
        ''' </summary>
        Public Async Function ValidateNewRegistrationAsync(
            learnerId As Guid, 
            targetSetaId As Guid, 
            targetProgrammeId As Guid) As Task(Of CrossSetaValidationResult)
            
            Dim result = New CrossSetaValidationResult()
            
            ' Get learner's existing contracts
            Dim existingContracts = Await _contractRepository.GetActiveContractsByLearnerIdAsync(learnerId)
            
            If existingContracts Is Nothing OrElse existingContracts.Count = 0 Then
                ' RULE: IF learner ID not found → REGISTER normally
                result.IsAllowed = True
                result.Action = ValidationAction.Allow
                result.Reason = "No existing contracts found. Registration allowed."
                Return result
            End If
            
            Dim targetProgramme = Await _programmeRepository.GetByIdAsync(targetProgrammeId)
            
            For Each existingContract In existingContracts
                ' Check if contract is in a different SETA
                If existingContract.SetaId <> targetSetaId Then
                    Dim existingProgramme = Await _programmeRepository.GetByIdAsync(existingContract.ProgrammeId)
                    
                    ' RULE: IF learner ID exists in another SETA with active funding for same qualification → BLOCK
                    If existingProgramme.QualificationCode = targetProgramme.QualificationCode Then
                        result.IsAllowed = False
                        result.Action = ValidationAction.Block
                        result.Reason = $"Learner has an active contract in another SETA ({existingContract.Seta?.Name}) for the same qualification ({existingProgramme.QualificationCode}). Double funding not allowed."
                        result.ConflictingContracts.Add(existingContract)
                        Return result
                    End If
                    
                    ' RULE: IF learner ID exists in another SETA with active funding but for different qualification → ALLOW, but FLAG for review
                    If existingProgramme.QualificationCode <> targetProgramme.QualificationCode Then
                        result.IsAllowed = True
                        result.Action = ValidationAction.AllowWithFlag
                        result.Reason = $"Learner has an active contract in another SETA ({existingContract.Seta?.Name}) for a different qualification. Allowed but flagged for review."
                        result.RequiresReview = True
                        result.ConflictingContracts.Add(existingContract)
                        Return result
                    End If
                End If
            Next
            
            ' Check for closed contracts in other SETAs
            Dim allContracts = Await _contractRepository.GetByLearnerIdAsync(learnerId)
            Dim closedContractsInOtherSetas = allContracts.Where(Function(c) c.SetaId <> targetSetaId AndAlso c.IsClosed()).ToList()
            
            If closedContractsInOtherSetas.Count > 0 Then
                ' RULE: IF learner ID exists but contract closed → ALLOW new registration, LINK records
                result.IsAllowed = True
                result.Action = ValidationAction.AllowAndLink
                result.Reason = $"Learner has closed contracts in other SETAs. Registration allowed and records will be linked."
                result.ShouldLinkRecords = True
                result.LinkedContracts.AddRange(closedContractsInOtherSetas)
                Return result
            End If
            
            ' Default: Allow registration
            result.IsAllowed = True
            result.Action = ValidationAction.Allow
            result.Reason = "No conflicts found. Registration allowed."
            
            Return result
        End Function
        
        ''' <summary>
        ''' Checks for potential funding fraud across SETAs
        ''' </summary>
        Public Async Function DetectCrossSetaFraudAsync(learnerId As Guid) As Task(Of List(Of FraudAlert))
            Dim alerts = New List(Of FraudAlert)()
            Dim activeContracts = Await _contractRepository.GetActiveContractsByLearnerIdAsync(learnerId)
            
            ' Group contracts by SETA
            Dim contractsBySeta = activeContracts.GroupBy(Function(c) c.SetaId).ToList()
            
            ' Check for multiple active contracts in different SETAs
            If contractsBySeta.Count > 1 Then
                For Each setaGroup In contractsBySeta
                    For Each contract In setaGroup
                        Dim programme = Await _programmeRepository.GetByIdAsync(contract.ProgrammeId)
                        
                        ' Check for same qualification in multiple SETAs
                        Dim duplicateQualification = activeContracts.Where(Function(c) 
                            c.SetaId <> contract.SetaId AndAlso 
                            c.Programme?.QualificationCode = programme.QualificationCode
                        ).ToList()
                        
                        If duplicateQualification.Count > 0 Then
                            alerts.Add(New FraudAlert() With {
                                .AlertType = "DUPLICATE_FUNDING",
                                .Severity = "HIGH",
                                .LearnerId = learnerId,
                                .Description = $"Learner is receiving funding for the same qualification ({programme.QualificationCode}) in multiple SETAs",
                                .ContractIds = New List(Of Guid) From {contract.Id}.Concat(duplicateQualification.Select(Function(c) c.Id)).ToList(),
                                .DetectedAt = DateTime.UtcNow
                            })
                        End If
                    Next
                Next
            End If
            
            ' Check for overlapping contract dates
            For i As Integer = 0 To activeContracts.Count - 1
                For j As Integer = i + 1 To activeContracts.Count - 1
                    If activeContracts(i).OverlapsWith(activeContracts(j)) Then
                        alerts.Add(New FraudAlert() With {
                            .AlertType = "OVERLAPPING_CONTRACTS",
                            .Severity = "MEDIUM",
                            .LearnerId = learnerId,
                            .Description = $"Learner has overlapping contracts (Contract {activeContracts(i).ContractNumber} and {activeContracts(j).ContractNumber})",
                            .ContractIds = New List(Of Guid) From {activeContracts(i).Id, activeContracts(j).Id},
                            .DetectedAt = DateTime.UtcNow
                        })
                    End If
                Next
            Next
            
            Return alerts
        End Function
        
        ''' <summary>
        ''' Gets learner's training history across all SETAs
        ''' </summary>
        Public Async Function GetLearnerHistoryAcrossSetasAsync(learnerId As Guid) As Task(Of LearnerSetaHistory)
            Dim contracts = Await _contractRepository.GetByLearnerIdAsync(learnerId)
            
            Dim history = New LearnerSetaHistory() With {
                .LearnerId = learnerId,
                .TotalContracts = contracts.Count,
                .ActiveContracts = contracts.Where(Function(c) c.IsCurrentlyActive()).Count(),
                .CompletedContracts = contracts.Where(Function(c) c.Status = ContractStatus.Completed).Count(),
                .TerminatedContracts = contracts.Where(Function(c) c.Status = ContractStatus.Terminated).Count(),
                .SetasInvolved = contracts.Select(Function(c) c.SetaId).Distinct().Count(),
                .ContractDetails = contracts.OrderByDescending(Function(c) c.StartDate).ToList()
            }
            
            Return history
        End Function
    End Class
    
    ''' <summary>
    ''' Result of cross-SETA validation
    ''' </summary>
    Public Class CrossSetaValidationResult
        Public Property IsAllowed As Boolean
        Public Property Action As ValidationAction
        Public Property Reason As String
        Public Property RequiresReview As Boolean
        Public Property ShouldLinkRecords As Boolean
        Public Property ConflictingContracts As List(Of Contract)
        Public Property LinkedContracts As List(Of Contract)
        
        Public Sub New()
            ConflictingContracts = New List(Of Contract)()
            LinkedContracts = New List(Of Contract)()
        End Sub
    End Class
    
    Public Enum ValidationAction
        Allow = 0
        AllowWithFlag = 1
        AllowAndLink = 2
        Block = 3
    End Enum
    
    Public Class FraudAlert
        Public Property AlertType As String
        Public Property Severity As String
        Public Property LearnerId As Guid
        Public Property Description As String
        Public Property ContractIds As List(Of Guid)
        Public Property DetectedAt As DateTime
    End Class
    
    Public Class LearnerSetaHistory
        Public Property LearnerId As Guid
        Public Property TotalContracts As Integer
        Public Property ActiveContracts As Integer
        Public Property CompletedContracts As Integer
        Public Property TerminatedContracts As Integer
        Public Property SetasInvolved As Integer
        Public Property ContractDetails As List(Of Contract)
    End Class
End Namespace
