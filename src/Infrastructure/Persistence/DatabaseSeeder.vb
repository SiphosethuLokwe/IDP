Imports System
Imports System.Threading.Tasks
Imports Microsoft.EntityFrameworkCore
Imports IDP.Domain.Entities
Imports IDP.Domain.Enums

Namespace Persistence
    Public Class DatabaseSeeder
        Private ReadOnly _context As ApplicationDbContext
        
        Public Sub New(context As ApplicationDbContext)
            _context = context
        End Sub
        
        Public Async Function SeedAsync() As Task
            Try
                Console.WriteLine("=== Starting Database Seeding ===")
                
                ' Check if data already exists
                Dim setaCount = Await _context.Setas.CountAsync()
                If setaCount > 0 Then
                    Console.WriteLine($"Database already seeded ({setaCount} SETAs found). Skipping seed.")
                    Return
                End If
                
                ' Seed SETAs
                Await SeedSetasAsync()
                
                ' Seed Sample Programmes
                Await SeedProgrammesAsync()
                
                ' Seed Sample Learners
                Await SeedLearnersAsync()
                
                ' Seed Sample Contracts
                Await SeedContractsAsync()
                
                ' Seed Duplication Rules
                Dim rulesSeeder = New DuplicationRulesSeeder(_context)
                Await rulesSeeder.SeedAsync()
                
                Console.WriteLine("=== Database Seeding Completed Successfully ===")
                
            Catch ex As Exception
                Console.WriteLine($"Error during seeding: {ex.Message}")
                Throw
            End Try
        End Function
        
        Private Async Function SeedSetasAsync() As Task
            Console.WriteLine("Seeding SETAs...")
            
            Dim setas = New List(Of Seta) From {
                New Seta() With {
                    .SetaCode = "AGRISETA",
                    .Name = "AGRISETA",
                    .FullName = "Agricultural Sector Education and Training Authority",
                    .Sector = "Agriculture and Nature Conservation",
                    .Description = "Covers primary agriculture, forestry, and nature conservation",
                    .ContactEmail = "info@agriseta.co.za",
                    .ContactPhone = "012 301 5600",
                    .Website = "www.agriseta.co.za",
                    .IsActive = True,
                    .EstablishedDate = New DateTime(2000, 3, 1)
                },
                New Seta() With {
                    .SetaCode = "BANKSETA",
                    .Name = "BANKSETA",
                    .FullName = "Banking Sector Education and Training Authority",
                    .Sector = "Banking, Financial Services, and Insurance",
                    .Description = "Covers banking, insurance, financial services, and stock broking",
                    .ContactEmail = "info@bankseta.org.za",
                    .ContactPhone = "011 805 9661",
                    .Website = "www.bankseta.org.za",
                    .IsActive = True,
                    .EstablishedDate = New DateTime(2000, 3, 1)
                },
                New Seta() With {
                    .SetaCode = "CATHSSETA",
                    .Name = "CATHSSETA",
                    .FullName = "Culture, Arts, Tourism, Hospitality and Sport Sector Education and Training Authority",
                    .Sector = "Culture, Arts, Tourism, Hospitality and Sport",
                    .Description = "Covers tourism, hospitality, sport, arts, and culture",
                    .ContactEmail = "info@cathsseta.org.za",
                    .ContactPhone = "011 217 0600",
                    .Website = "www.cathsseta.org.za",
                    .IsActive = True,
                    .EstablishedDate = New DateTime(2000, 3, 1)
                },
                New Seta() With {
                    .SetaCode = "CHIETA",
                    .Name = "CHIETA",
                    .FullName = "Chemical Industries Education and Training Authority",
                    .Sector = "Chemical Industries",
                    .Description = "Covers chemicals, plastics, petroleum, and related industries",
                    .ContactEmail = "info@chieta.org.za",
                    .ContactPhone = "011 726 6120",
                    .Website = "www.chieta.org.za",
                    .IsActive = True,
                    .EstablishedDate = New DateTime(2000, 3, 1)
                },
                New Seta() With {
                    .SetaCode = "CETA",
                    .Name = "CETA",
                    .FullName = "Construction Education and Training Authority",
                    .Sector = "Construction",
                    .Description = "Covers construction, building, and civil engineering",
                    .ContactEmail = "info@ceta.org.za",
                    .ContactPhone = "011 265 5900",
                    .Website = "www.ceta.org.za",
                    .IsActive = True,
                    .EstablishedDate = New DateTime(2000, 3, 1)
                },
                New Seta() With {
                    .SetaCode = "ETDPSETA",
                    .Name = "ETDP SETA",
                    .FullName = "Education, Training and Development Practices Sector Education and Training Authority",
                    .Sector = "Education, Training and Development",
                    .Description = "Covers education and training services",
                    .ContactEmail = "info@etdpseta.org.za",
                    .ContactPhone = "011 372 3300",
                    .Website = "www.etdpseta.org.za",
                    .IsActive = True,
                    .EstablishedDate = New DateTime(2000, 3, 1)
                },
                New Seta() With {
                    .SetaCode = "EWSETA",
                    .Name = "EWSETA",
                    .FullName = "Energy and Water Sector Education and Training Authority",
                    .Sector = "Energy and Water",
                    .Description = "Covers electricity, water, and alternative energy",
                    .ContactEmail = "info@ewseta.org.za",
                    .ContactPhone = "011 274 5000",
                    .Website = "www.ewseta.org.za",
                    .IsActive = True,
                    .EstablishedDate = New DateTime(2000, 3, 1)
                },
                New Seta() With {
                    .SetaCode = "FOODBEV",
                    .Name = "FoodBev SETA",
                    .FullName = "Food and Beverages Manufacturing Sector Education and Training Authority",
                    .Sector = "Food and Beverages",
                    .Description = "Covers food processing, beverages, and related manufacturing",
                    .ContactEmail = "info@foodbev.co.za",
                    .ContactPhone = "011 253 7300",
                    .Website = "www.foodbev.co.za",
                    .IsActive = True,
                    .EstablishedDate = New DateTime(2000, 3, 1)
                },
                New Seta() With {
                    .SetaCode = "FASSET",
                    .Name = "FASSET",
                    .FullName = "Finance and Accounting Services Sector Education and Training Authority",
                    .Sector = "Finance and Accounting",
                    .Description = "Covers accounting, auditing, and financial management",
                    .ContactEmail = "info@fasset.org.za",
                    .ContactPhone = "011 476 8570",
                    .Website = "www.fasset.org.za",
                    .IsActive = True,
                    .EstablishedDate = New DateTime(2000, 3, 1)
                },
                New Seta() With {
                    .SetaCode = "FIETA",
                    .Name = "FIETA",
                    .FullName = "Forest Industries Education and Training Authority",
                    .Sector = "Forestry and Wood Products",
                    .Description = "Covers forestry, timber, pulp, paper, and furniture",
                    .ContactEmail = "info@fieta.org.za",
                    .ContactPhone = "011 253 7140",
                    .Website = "www.fieta.org.za",
                    .IsActive = True,
                    .EstablishedDate = New DateTime(2000, 3, 1)
                },
                New Seta() With {
                    .SetaCode = "HWSETA",
                    .Name = "HWSETA",
                    .FullName = "Health and Welfare Sector Education and Training Authority",
                    .Sector = "Health and Social Services",
                    .Description = "Covers health services, social services, and welfare",
                    .ContactEmail = "info@hwseta.org.za",
                    .ContactPhone = "011 607 6947",
                    .Website = "www.hwseta.org.za",
                    .IsActive = True,
                    .EstablishedDate = New DateTime(2000, 3, 1)
                },
                New Seta() With {
                    .SetaCode = "INSETA",
                    .Name = "INSETA",
                    .FullName = "Insurance Sector Education and Training Authority",
                    .Sector = "Insurance and Retirement Funds",
                    .Description = "Covers insurance, retirement funds, and related services",
                    .ContactEmail = "info@inseta.org.za",
                    .ContactPhone = "011 544 3000",
                    .Website = "www.inseta.org.za",
                    .IsActive = True,
                    .EstablishedDate = New DateTime(2000, 3, 1)
                },
                New Seta() With {
                    .SetaCode = "LGSETA",
                    .Name = "LGSETA",
                    .FullName = "Local Government Sector Education and Training Authority",
                    .Sector = "Local Government",
                    .Description = "Covers municipalities and local government services",
                    .ContactEmail = "info@lgseta.co.za",
                    .ContactPhone = "011 456 8579",
                    .Website = "www.lgseta.co.za",
                    .IsActive = True,
                    .EstablishedDate = New DateTime(2000, 3, 1)
                },
                New Seta() With {
                    .SetaCode = "MAPPP",
                    .Name = "MAPPP-SETA",
                    .FullName = "Media, Advertising, Publishing, Printing and Packaging Sector Education and Training Authority",
                    .Sector = "Media and Publishing",
                    .Description = "Covers media, advertising, publishing, printing, and packaging",
                    .ContactEmail = "info@mappp-seta.co.za",
                    .ContactPhone = "011 544 6940",
                    .Website = "www.mappp-seta.co.za",
                    .IsActive = True,
                    .EstablishedDate = New DateTime(2000, 3, 1)
                },
                New Seta() With {
                    .SetaCode = "MERSETA",
                    .Name = "merSETA",
                    .FullName = "Manufacturing, Engineering and Related Services Sector Education and Training Authority",
                    .Sector = "Manufacturing and Engineering",
                    .Description = "Covers metals, engineering, motor, plastics, and related sectors",
                    .ContactEmail = "info@merseta.org.za",
                    .ContactPhone = "011 484 0311",
                    .Website = "www.merseta.org.za",
                    .IsActive = True,
                    .EstablishedDate = New DateTime(2000, 3, 1)
                },
                New Seta() With {
                    .SetaCode = "MICT",
                    .Name = "MICT SETA",
                    .FullName = "Media, Information and Communication Technologies Sector Education and Training Authority",
                    .Sector = "ICT and Electronics",
                    .Description = "Covers information technology, telecommunications, and electronics",
                    .ContactEmail = "info@mict.org.za",
                    .ContactPhone = "011 207 2600",
                    .Website = "www.mict.org.za",
                    .IsActive = True,
                    .EstablishedDate = New DateTime(2000, 3, 1)
                },
                New Seta() With {
                    .SetaCode = "PSETA",
                    .Name = "PSETA",
                    .FullName = "Public Service Sector Education and Training Authority",
                    .Sector = "Public Service",
                    .Description = "Covers public service and government departments",
                    .ContactEmail = "info@pseta.gov.za",
                    .ContactPhone = "012 423 5144",
                    .Website = "www.pseta.gov.za",
                    .IsActive = True,
                    .EstablishedDate = New DateTime(2000, 3, 1)
                },
                New Seta() With {
                    .SetaCode = "SASSETA",
                    .Name = "SASSETA",
                    .FullName = "Safety and Security Sector Education and Training Authority",
                    .Sector = "Safety and Security",
                    .Description = "Covers police, defense, correctional services, and private security",
                    .ContactEmail = "info@sasseta.org.za",
                    .ContactPhone = "012 347 8490",
                    .Website = "www.sasseta.org.za",
                    .IsActive = True,
                    .EstablishedDate = New DateTime(2000, 3, 1)
                },
                New Seta() With {
                    .SetaCode = "SERVICES",
                    .Name = "Services SETA",
                    .FullName = "Services Sector Education and Training Authority",
                    .Sector = "Services",
                    .Description = "Covers wholesale, retail, motor trade, and related services",
                    .ContactEmail = "info@serviceseta.org.za",
                    .ContactPhone = "011 276 9600",
                    .Website = "www.serviceseta.org.za",
                    .IsActive = True,
                    .EstablishedDate = New DateTime(2000, 3, 1)
                },
                New Seta() With {
                    .SetaCode = "TETA",
                    .Name = "TETA",
                    .FullName = "Transport Education and Training Authority",
                    .Sector = "Transport",
                    .Description = "Covers road, rail, air, sea transport and related logistics",
                    .ContactEmail = "info@teta.org.za",
                    .ContactPhone = "011 781 1280",
                    .Website = "www.teta.org.za",
                    .IsActive = True,
                    .EstablishedDate = New DateTime(2000, 3, 1)
                },
                New Seta() With {
                    .SetaCode = "WSETA",
                    .Name = "W&RSETA",
                    .FullName = "Wholesale and Retail Sector Education and Training Authority",
                    .Sector = "Wholesale and Retail",
                    .Description = "Covers wholesale, retail trade, and related services",
                    .ContactEmail = "info@wrseta.org.za",
                    .ContactPhone = "011 501 9820",
                    .Website = "www.wrseta.org.za",
                    .IsActive = True,
                    .EstablishedDate = New DateTime(2000, 3, 1)
                }
            }
            
            _context.Setas.AddRange(setas)
            Await _context.SaveChangesAsync()
            
            Console.WriteLine($"✓ Seeded {setas.Count} SETAs")
        End Function
        
        Private Async Function SeedProgrammesAsync() As Task
            Console.WriteLine("Seeding Programmes...")
            
            Dim mictSeta = Await _context.Setas.FirstOrDefaultAsync(Function(s) s.SetaCode = "MICT")
            Dim merSeta = Await _context.Setas.FirstOrDefaultAsync(Function(s) s.SetaCode = "MERSETA")
            Dim hwSeta = Await _context.Setas.FirstOrDefaultAsync(Function(s) s.SetaCode = "HWSETA")
            
            Dim programmes = New List(Of Programme) From {
                New Programme() With {
                    .QualificationCode = "Q0001",
                    .Title = "National Certificate: Information Technology Systems Development",
                    .Description = "Software development and programming qualification",
                    .NQFLevel = 5,
                    .Credits = 120,
                    .Sector = "ICT",
                    .ProgrammeType = "Learnership",
                    .Duration = 12,
                    .SetaId = If(mictSeta IsNot Nothing, mictSeta.Id, CType(Nothing, Guid?)),
                    .IsActive = True
                },
                New Programme() With {
                    .QualificationCode = "Q0002",
                    .Title = "National Certificate: IT Technical Support",
                    .Description = "IT support and helpdesk qualification",
                    .NQFLevel = 4,
                    .Credits = 120,
                    .Sector = "ICT",
                    .ProgrammeType = "Learnership",
                    .Duration = 12,
                    .SetaId = If(mictSeta IsNot Nothing, mictSeta.Id, CType(Nothing, Guid?)),
                    .IsActive = True
                },
                New Programme() With {
                    .QualificationCode = "Q0003",
                    .Title = "National Diploma: Engineering",
                    .Description = "Mechanical and electrical engineering qualification",
                    .NQFLevel = 6,
                    .Credits = 360,
                    .Sector = "Engineering",
                    .ProgrammeType = "Learnership",
                    .Duration = 36,
                    .SetaId = If(merSeta IsNot Nothing, merSeta.Id, CType(Nothing, Guid?)),
                    .IsActive = True
                },
                New Programme() With {
                    .QualificationCode = "Q0004",
                    .Title = "National Certificate: Nursing Assistant",
                    .Description = "Healthcare and nursing assistant qualification",
                    .NQFLevel = 4,
                    .Credits = 120,
                    .Sector = "Healthcare",
                    .ProgrammeType = "Learnership",
                    .Duration = 12,
                    .SetaId = If(hwSeta IsNot Nothing, hwSeta.Id, CType(Nothing, Guid?)),
                    .IsActive = True
                },
                New Programme() With {
                    .QualificationCode = "Q0005",
                    .Title = "National Certificate: Project Management",
                    .Description = "Project management and coordination qualification",
                    .NQFLevel = 5,
                    .Credits = 120,
                    .Sector = "Business",
                    .ProgrammeType = "Skills Programme",
                    .Duration = 6,
                    .SetaId = Nothing,
                    .IsActive = True
                }
            }
            
            _context.Programmes.AddRange(programmes)
            Await _context.SaveChangesAsync()
            
            Console.WriteLine($"✓ Seeded {programmes.Count} Programmes")
        End Function
        
        Private Async Function SeedLearnersAsync() As Task
            Console.WriteLine("Seeding Sample Learners...")
            
            Dim learners = New List(Of Learner) From {
                New Learner() With {
                    .IdNumber = "9501015800081",
                    .FirstName = "Thabo",
                    .LastName = "Mbeki",
                    .DateOfBirth = New DateTime(1995, 1, 1),
                    .PhoneNumber = "0821234567",
                    .Email = "thabo.mbeki@example.com",
                    .SetaCode = "MICT",
                    .IsActive = True
                },
                New Learner() With {
                    .IdNumber = "9802155600082",
                    .FirstName = "Nomsa",
                    .LastName = "Dlamini",
                    .DateOfBirth = New DateTime(1998, 2, 15),
                    .PhoneNumber = "0832345678",
                    .Email = "nomsa.dlamini@example.com",
                    .SetaCode = "HWSETA",
                    .IsActive = True
                },
                New Learner() With {
                    .IdNumber = "0005125800083",
                    .FirstName = "Sipho",
                    .LastName = "Nkosi",
                    .DateOfBirth = New DateTime(2000, 5, 12),
                    .PhoneNumber = "0843456789",
                    .Email = "sipho.nkosi@example.com",
                    .SetaCode = "MERSETA",
                    .IsActive = True
                },
                New Learner() With {
                    .IdNumber = "9707085600084",
                    .FirstName = "Lerato",
                    .LastName = "Mokoena",
                    .DateOfBirth = New DateTime(1997, 7, 8),
                    .PhoneNumber = "0784567890",
                    .Email = "lerato.mokoena@example.com",
                    .SetaCode = "MICT",
                    .IsActive = True
                },
                New Learner() With {
                    .IdNumber = "9909215800085",
                    .FirstName = "Mandla",
                    .LastName = "Khumalo",
                    .DateOfBirth = New DateTime(1999, 9, 21),
                    .PhoneNumber = "0735678901",
                    .Email = "mandla.khumalo@example.com",
                    .SetaCode = "SERVICES",
                    .IsActive = True
                }
            }
            
            _context.Learners.AddRange(learners)
            Await _context.SaveChangesAsync()
            
            Console.WriteLine($"✓ Seeded {learners.Count} Sample Learners")
        End Function
        
        Private Async Function SeedContractsAsync() As Task
            Console.WriteLine("Seeding Sample Contracts...")
            
            Dim learner1 = Await _context.Learners.FirstOrDefaultAsync(Function(l) l.IdNumber = "9501015800081")
            Dim learner2 = Await _context.Learners.FirstOrDefaultAsync(Function(l) l.IdNumber = "9802155600082")
            Dim programme1 = Await _context.Programmes.FirstOrDefaultAsync(Function(p) p.QualificationCode = "Q0001")
            Dim programme2 = Await _context.Programmes.FirstOrDefaultAsync(Function(p) p.QualificationCode = "Q0004")
            Dim mictSeta = Await _context.Setas.FirstOrDefaultAsync(Function(s) s.SetaCode = "MICT")
            Dim hwSeta = Await _context.Setas.FirstOrDefaultAsync(Function(s) s.SetaCode = "HWSETA")
            
            If learner1 Is Nothing OrElse learner2 Is Nothing OrElse programme1 Is Nothing OrElse programme2 Is Nothing Then
                Console.WriteLine("⚠ Skipping contracts - missing dependencies")
                Return
            End If
            
            Dim contracts = New List(Of Contract) From {
                New Contract() With {
                    .ContractNumber = "CNT-2024-001",
                    .LearnerId = learner1.Id,
                    .SetaId = mictSeta.Id,
                    .ProgrammeId = programme1.Id,
                    .ProviderId = "PROV001",
                    .ProviderName = "TechSkills Training Academy",
                    .Status = ContractStatus.Active,
                    .StartDate = DateTime.UtcNow.AddMonths(-3),
                    .EndDate = DateTime.UtcNow.AddMonths(9),
                    .FundingAmount = 35000D,
                    .IsActive = True,
                    .CreatedBy = "System Seeder"
                },
                New Contract() With {
                    .ContractNumber = "CNT-2024-002",
                    .LearnerId = learner2.Id,
                    .SetaId = hwSeta.Id,
                    .ProgrammeId = programme2.Id,
                    .ProviderId = "PROV002",
                    .ProviderName = "Healthcare Training Institute",
                    .Status = ContractStatus.Active,
                    .StartDate = DateTime.UtcNow.AddMonths(-2),
                    .EndDate = DateTime.UtcNow.AddMonths(10),
                    .FundingAmount = 28000D,
                    .IsActive = True,
                    .CreatedBy = "System Seeder"
                }
            }
            
            _context.Contracts.AddRange(contracts)
            Await _context.SaveChangesAsync()
            
            Console.WriteLine($"✓ Seeded {contracts.Count} Sample Contracts")
        End Function
    End Class
End Namespace
