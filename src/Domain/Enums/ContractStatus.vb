Namespace Enums
    ''' <summary>
    ''' Represents the status of a learner's contract/learnership with a SETA.
    ''' Critical for duplicate detection and funding validation.
    ''' </summary>
    Public Enum ContractStatus
        ''' <summary>
        ''' Contract is pending approval or start
        ''' </summary>
        Pending = 0
        
        ''' <summary>
        ''' Contract is active and learner is currently funded
        ''' </summary>
        Active = 1
        
        ''' <summary>
        ''' Contract is on hold/suspended temporarily
        ''' </summary>
        Suspended = 2
        
        ''' <summary>
        ''' Learner successfully completed the programme
        ''' </summary>
        Completed = 3
        
        ''' <summary>
        ''' Learner terminated/dropped out before completion
        ''' </summary>
        Terminated = 4
        
        ''' <summary>
        ''' Contract was cancelled before it started
        ''' </summary>
        Cancelled = 5
        
        ''' <summary>
        ''' Contract expired without completion
        ''' </summary>
        Expired = 6
    End Enum
End Namespace
