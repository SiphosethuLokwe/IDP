Imports System

Namespace ML

    ''' <summary>
    ''' Fuzzy string matching algorithms for detecting similar but not identical data
    ''' Critical for handling typos, data entry errors, and minor variations
    ''' </summary>
    Public Class FuzzyMatchingService

        ''' <summary>
        ''' Calculates Levenshtein Distance - minimum number of edits to transform one string to another
        ''' Used for: Names, ID numbers with typos, addresses
        ''' Example: "Sipho" vs "Sipo" = distance of 1
        ''' </summary>
        Public Function LevenshteinDistance(source As String, target As String) As Integer
            If String.IsNullOrEmpty(source) Then Return If(String.IsNullOrEmpty(target), 0, target.Length)
            If String.IsNullOrEmpty(target) Then Return source.Length

            Dim sourceLength = source.Length
            Dim targetLength = target.Length
            Dim distance(sourceLength, targetLength) As Integer

            ' Initialize first row and column
            For i = 0 To sourceLength
                distance(i, 0) = i
            Next
            For j = 0 To targetLength
                distance(0, j) = j
            Next

            ' Calculate distance
            For i = 1 To sourceLength
                For j = 1 To targetLength
                    Dim cost = If(source(i - 1) = target(j - 1), 0, 1)
                    distance(i, j) = Math.Min(
                        Math.Min(
                            distance(i - 1, j) + 1,      ' Deletion
                            distance(i, j - 1) + 1),     ' Insertion
                        distance(i - 1, j - 1) + cost)   ' Substitution
                Next
            Next

            Return distance(sourceLength, targetLength)
        End Function

        ''' <summary>
        ''' Calculates similarity score (0.0 to 1.0) based on Levenshtein distance
        ''' 1.0 = identical, 0.0 = completely different
        ''' </summary>
        Public Function CalculateSimilarity(source As String, target As String) As Single
            If String.IsNullOrEmpty(source) AndAlso String.IsNullOrEmpty(target) Then Return 1.0F
            If String.IsNullOrEmpty(source) OrElse String.IsNullOrEmpty(target) Then Return 0.0F

            Dim maxLength = Math.Max(source.Length, target.Length)
            Dim distance = LevenshteinDistance(source.ToUpper(), target.ToUpper())
            
            Return CSng(1.0 - (distance / maxLength))
        End Function

        ''' <summary>
        ''' Double Metaphone phonetic algorithm - encodes names by how they sound
        ''' Critical for: Handling different spellings of same-sounding names
        ''' Example: "Stephen" and "Steven" both encode to "STFN"
        ''' Example: "Smith" and "Smythe" sound similar
        ''' </summary>
        Public Function GetPhoneticCode(input As String) As String
            If String.IsNullOrEmpty(input) Then Return String.Empty
            
            ' Simplified Metaphone implementation
            ' For production, use a library like "DoubleMetaphone" NuGet package
            Dim cleaned = input.ToUpper().Trim()
            Dim result = New System.Text.StringBuilder()
            
            For i = 0 To cleaned.Length - 1
                Dim c = cleaned(i)
                
                ' Remove vowels except at start
                If i = 0 OrElse Not IsVowel(c) Then
                    Select Case c
                        Case "C"c
                            ' CH -> X
                            If i + 1 < cleaned.Length AndAlso cleaned(i + 1) = "H"c Then
                                result.Append("X")
                            Else
                                result.Append("K")
                            End If
                        Case "P"c
                            ' PH -> F
                            If i + 1 < cleaned.Length AndAlso cleaned(i + 1) = "H"c Then
                                result.Append("F")
                            Else
                                result.Append("P")
                            End If
                        Case "G"c
                            result.Append("K")
                        Case "Q"c
                            result.Append("K")
                        Case "S"c
                            ' SH -> X
                            If i + 1 < cleaned.Length AndAlso cleaned(i + 1) = "H"c Then
                                result.Append("X")
                            Else
                                result.Append("S")
                            End If
                        Case "Z"c
                            result.Append("S")
                        Case Else
                            If Not IsVowel(c) Then
                                result.Append(c)
                            End If
                    End Select
                End If
            Next
            
            Return result.ToString()
        End Function

        ''' <summary>
        ''' Compares phonetic codes to detect similar-sounding names
        ''' Returns 1.0 if they sound the same, 0.0 if completely different
        ''' </summary>
        Public Function PhoneticMatch(name1 As String, name2 As String) As Single
            If String.IsNullOrEmpty(name1) OrElse String.IsNullOrEmpty(name2) Then Return 0.0F
            
            Dim code1 = GetPhoneticCode(name1)
            Dim code2 = GetPhoneticCode(name2)
            
            If code1 = code2 Then Return 1.0F
            
            ' If codes are similar but not identical, use similarity
            Return CalculateSimilarity(code1, code2)
        End Function

        ''' <summary>
        ''' Jaro-Winkler similarity - gives more weight to matching prefixes
        ''' Better for: Short strings like names, codes
        ''' Example: "Martha" vs "Marhta" = high similarity (transposition)
        ''' </summary>
        Public Function JaroWinklerSimilarity(s1 As String, s2 As String) As Single
            If String.IsNullOrEmpty(s1) AndAlso String.IsNullOrEmpty(s2) Then Return 1.0F
            If String.IsNullOrEmpty(s1) OrElse String.IsNullOrEmpty(s2) Then Return 0.0F
            
            Dim jaroSim = JaroSimilarity(s1, s2)
            
            ' Calculate common prefix length (up to 4 characters)
            Dim prefixLength = 0
            Dim maxPrefix = Math.Min(4, Math.Min(s1.Length, s2.Length))
            
            For i = 0 To maxPrefix - 1
                If s1(i) = s2(i) Then
                    prefixLength += 1
                Else
                    Exit For
                End If
            Next
            
            ' Jaro-Winkler gives bonus for matching prefix
            Dim p As Single = 0.1F ' Scaling factor
            Return jaroSim + (prefixLength * p * (1 - jaroSim))
        End Function

        Private Function JaroSimilarity(s1 As String, s2 As String) As Single
            Dim len1 = s1.Length
            Dim len2 = s2.Length
            
            If len1 = 0 AndAlso len2 = 0 Then Return 1.0F
            If len1 = 0 OrElse len2 = 0 Then Return 0.0F
            
            Dim matchDistance = Math.Max(len1, len2) \ 2 - 1
            Dim s1Matches = New Boolean(len1 - 1) {}
            Dim s2Matches = New Boolean(len2 - 1) {}
            
            Dim matches = 0
            Dim transpositions = 0
            
            ' Find matches
            For i = 0 To len1 - 1
                Dim start = Math.Max(0, i - matchDistance)
                Dim [end] = Math.Min(i + matchDistance + 1, len2)
                
                For j = start To [end] - 1
                    If s2Matches(j) Then Continue For
                    If s1(i) <> s2(j) Then Continue For
                    s1Matches(i) = True
                    s2Matches(j) = True
                    matches += 1
                    Exit For
                Next
            Next
            
            If matches = 0 Then Return 0.0F
            
            ' Count transpositions
            Dim k = 0
            For i = 0 To len1 - 1
                If Not s1Matches(i) Then Continue For
                While Not s2Matches(k)
                    k += 1
                End While
                If s1(i) <> s2(k) Then transpositions += 1
                k += 1
            Next
            
            Return CSng((matches / len1 + matches / len2 + (matches - transpositions / 2) / matches) / 3.0)
        End Function

        Private Function IsVowel(c As Char) As Boolean
            Return "AEIOU".Contains(c)
        End Function

        ''' <summary>
        ''' Smart ID number comparison - handles transpositions and common typos
        ''' South African ID: YYMMDDGSSSCAZ (13 digits)
        ''' Returns weighted similarity considering each section
        ''' </summary>
        Public Function CompareIdNumbers(id1 As String, id2 As String) As Single
            If String.IsNullOrEmpty(id1) OrElse String.IsNullOrEmpty(id2) Then Return 0.0F
            If id1 = id2 Then Return 1.0F
            
            ' If both are 13 digits (SA ID format), do smart comparison
            If id1.Length = 13 AndAlso id2.Length = 13 AndAlso 
               IsNumeric(id1) AndAlso IsNumeric(id2) Then
                
                Dim score As Single = 0.0F
                
                ' Date of birth section (YYMMDD) - weight 0.4
                Dim dob1 = id1.Substring(0, 6)
                Dim dob2 = id2.Substring(0, 6)
                If dob1 = dob2 Then score += 0.4F
                
                ' Gender (G) - weight 0.1
                If id1(6) = id2(6) Then score += 0.1F
                
                ' Sequence (SSS) - weight 0.2
                Dim seq1 = id1.Substring(7, 3)
                Dim seq2 = id2.Substring(7, 3)
                If seq1 = seq2 Then score += 0.2F
                
                ' Citizenship (C) - weight 0.1
                If id1(10) = id2(10) Then score += 0.1F
                
                ' Check digits - weight 0.2
                Dim check1 = id1.Substring(11, 2)
                Dim check2 = id2.Substring(11, 2)
                If check1 = check2 Then score += 0.2F
                
                Return score
            End If
            
            ' Fallback to Levenshtein for non-standard IDs
            Return CalculateSimilarity(id1, id2)
        End Function

        Private Function IsNumeric(value As String) As Boolean
            Return value.All(AddressOf Char.IsDigit)
        End Function

    End Class

End Namespace
