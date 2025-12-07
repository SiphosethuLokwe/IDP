Imports System
Imports System.Threading.Tasks
Imports System.Text.Json
Imports Microsoft.EntityFrameworkCore
Imports IDP.Domain.Entities
Imports RulesEngine.Models

Namespace Persistence
    ''' <summary>
    ''' Seeds default duplication detection rules for the RulesEngine fallback
    ''' </summary>
    Public Class DuplicationRulesSeeder
        Private ReadOnly _context As ApplicationDbContext
        
        Public Sub New(context As ApplicationDbContext)
            _context = context
        End Sub
        
        Public Async Function SeedAsync() As Task
            ' Check if rules already exist
            Dim rulesCount = Await _context.DuplicationRules.CountAsync()
            If rulesCount > 0 Then
                Console.WriteLine($"Duplication rules already seeded ({rulesCount} rules found). Skipping.")
                Return
            End If
            
            Console.WriteLine("Seeding duplication detection rules...")
            
            Dim rules = New List(Of DuplicationRule)()
            
            ' Rule 1: Exact ID Match (Highest Priority)
            rules.Add(CreateRule(
                "Exact ID Number Match",
                "Flags duplicate when ID numbers match exactly",
                CreateExactIdMatchWorkflow(),
                100,
                1.0D))
            
            ' Rule 2: Exact Name + DOB Match
            rules.Add(CreateRule(
                "Exact Name and Date of Birth Match",
                "Flags duplicate when full name and date of birth match exactly",
                CreateNameAndDobMatchWorkflow(),
                90,
                0.95D))
            
            ' Rule 3: Name + DOB + SETA Match
            rules.Add(CreateRule(
                "Name, DOB and SETA Match",
                "Flags duplicate when name, DOB and SETA code match",
                CreateNameDobSetaMatchWorkflow(),
                85,
                0.90D))
            
            ' Rule 4: Name + Phone Match
            rules.Add(CreateRule(
                "Name and Phone Match",
                "Flags duplicate when full name and phone number match",
                CreateNamePhoneMatchWorkflow(),
                80,
                0.85D))
            
            ' Rule 5: Email Match (Different Name)
            rules.Add(CreateRule(
                "Email Match with Different Name",
                "Flags potential duplicate when email matches but names differ (possible typo)",
                CreateEmailMatchWorkflow(),
                70,
                0.75D))
            
            Await _context.DuplicationRules.AddRangeAsync(rules)
            Await _context.SaveChangesAsync()
            
            Console.WriteLine($"✓ Seeded {rules.Count} duplication detection rules")
        End Function
        
        Private Function CreateRule(name As String, description As String, workflow As Workflow(), priority As Integer, minConfidence As Decimal) As DuplicationRule
            Return New DuplicationRule() With {
                .RuleName = name,
                .RuleDescription = description,
                .RuleJson = JsonSerializer.Serialize(workflow),
                .Priority = priority,
                .MinConfidenceScore = minConfidence,
                .IsActive = True,
                .CreatedAt = DateTime.UtcNow
            }
        End Function
        
        Private Function CreateExactIdMatchWorkflow() As Workflow()
            Return New Workflow() {
                New Workflow() With {
                    .WorkflowName = "ExactIdMatch",
                    .Rules = New Rule() {
                        New Rule() With {
                            .RuleName = "ExactIdNumberMatch",
                            .SuccessEvent = "Duplicate detected: Exact ID match",
                            .ErrorMessage = "No ID match",
                            .RuleExpressionType = RuleExpressionType.LambdaExpression,
                            .Expression = "idMatch == true"
                        }
                    }
                }
            }
        End Function
        
        Private Function CreateNameAndDobMatchWorkflow() As Workflow()
            Return New Workflow() {
                New Workflow() With {
                    .WorkflowName = "NameAndDobMatch",
                    .Rules = New Rule() {
                        New Rule() With {
                            .RuleName = "NameDobMatch",
                            .SuccessEvent = "Duplicate detected: Name and DOB match",
                            .ErrorMessage = "No name+DOB match",
                            .RuleExpressionType = RuleExpressionType.LambdaExpression,
                            .Expression = "nameMatch == true && dobMatch == true"
                        }
                    }
                }
            }
        End Function
        
        Private Function CreateNameDobSetaMatchWorkflow() As Workflow()
            Return New Workflow() {
                New Workflow() With {
                    .WorkflowName = "NameDobSetaMatch",
                    .Rules = New Rule() {
                        New Rule() With {
                            .RuleName = "NameDobSetaMatch",
                            .SuccessEvent = "Duplicate detected: Name, DOB and SETA match",
                            .ErrorMessage = "No match",
                            .RuleExpressionType = RuleExpressionType.LambdaExpression,
                            .Expression = "nameMatch == true && dobMatch == true && sameSeta == true"
                        }
                    }
                }
            }
        End Function
        
        Private Function CreateNamePhoneMatchWorkflow() As Workflow()
            Return New Workflow() {
                New Workflow() With {
                    .WorkflowName = "NamePhoneMatch",
                    .Rules = New Rule() {
                        New Rule() With {
                            .RuleName = "NamePhoneMatch",
                            .SuccessEvent = "Duplicate detected: Name and phone match",
                            .ErrorMessage = "No match",
                            .RuleExpressionType = RuleExpressionType.LambdaExpression,
                            .Expression = "nameMatch == true && phoneMatch == true"
                        }
                    }
                }
            }
        End Function
        
        Private Function CreateEmailMatchWorkflow() As Workflow()
            Return New Workflow() {
                New Workflow() With {
                    .WorkflowName = "EmailMatch",
                    .Rules = New Rule() {
                        New Rule() With {
                            .RuleName = "EmailMatchDifferentName",
                            .SuccessEvent = "Potential duplicate: Email match with different name",
                            .ErrorMessage = "No match",
                            .RuleExpressionType = RuleExpressionType.LambdaExpression,
                            .Expression = "emailMatch == true && nameMatch == false"
                        }
                    }
                }
            }
        End Function
    End Class
End Namespace
