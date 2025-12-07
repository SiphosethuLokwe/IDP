Namespace Enums
    Public Enum MatchType
        ExactIdMatch = 1
        PartialIdMatch = 2
        NameAndDobMatch = 3
        PhoneNumberMatch = 4
        EmailMatch = 5
        PassportMatch = 6
        FuzzyMatch = 7
        ExternalVerificationMatch = 8
        AIMatch = 9 ' ML.NET AI prediction
        RuleMatch = 10 ' RulesEngine fallback match
    End Enum
End Namespace
