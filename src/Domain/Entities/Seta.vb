Imports System

Namespace Entities
    ''' <summary>
    ''' Represents a Sector Education and Training Authority (SETA) in South Africa.
    ''' There are 21 SETAs, each responsible for skills development in specific industry sectors.
    ''' </summary>
    Public Class Seta
        Public Property Id As Guid
        Public Property SetaCode As String ' e.g., "BANKSETA", "CETA", "CHIETA"
        Public Property Name As String
        Public Property FullName As String
        Public Property Sector As String
        Public Property Description As String
        Public Property ContactEmail As String
        Public Property ContactPhone As String
        Public Property Website As String
        Public Property IsActive As Boolean
        Public Property EstablishedDate As Date?
        Public Property CreatedAt As DateTime
        Public Property UpdatedAt As DateTime?
        
        ' Navigation properties
        Public Property Contracts As List(Of Contract)
        
        Public Sub New()
            Id = Guid.NewGuid()
            CreatedAt = DateTime.UtcNow
            IsActive = True
            Contracts = New List(Of Contract)()
        End Sub
        
        ''' <summary>
        ''' Seeds the 21 official South African SETAs as of 2025
        ''' </summary>
        Public Shared Function GetAllSetas() As List(Of Seta)
            Return New List(Of Seta) From {
                New Seta With {
                    .SetaCode = "AGRISETA",
                    .Name = "AGRISETA",
                    .FullName = "Agriculture Sector Education and Training Authority",
                    .Sector = "Agriculture and Related Services",
                    .Description = "Primary and secondary agriculture, forestry, and fishing"
                },
                New Seta With {
                    .SetaCode = "BANKSETA",
                    .Name = "BANKSETA",
                    .FullName = "Banking Sector Education and Training Authority",
                    .Sector = "Banking, Financial Services, and Insurance",
                    .Description = "Banking, microfinance, financial services, insurance"
                },
                New Seta With {
                    .SetaCode = "CHIETA",
                    .Name = "CHIETA",
                    .FullName = "Chemical Industries Education and Training Authority",
                    .Sector = "Chemical Industries",
                    .Description = "Chemical, petroleum, plastics, and glass manufacturing"
                },
                New Seta With {
                    .SetaCode = "CTFL",
                    .Name = "CTFL",
                    .FullName = "Clothing, Textiles, Footwear and Leather SETA",
                    .Sector = "Clothing, Textiles, Footwear and Leather",
                    .Description = "Textile, clothing, footwear, and leather goods manufacturing"
                },
                New Seta With {
                    .SetaCode = "CETA",
                    .Name = "CETA",
                    .FullName = "Construction Education and Training Authority",
                    .Sector = "Construction and Built Environment",
                    .Description = "Building construction, civil engineering, electrical infrastructure"
                },
                New Seta With {
                    .SetaCode = "ETDPSETA",
                    .Name = "ETDP SETA",
                    .FullName = "Education, Training and Development Practices SETA",
                    .Sector = "Education, Training and Development",
                    .Description = "Education and training services, early childhood development"
                },
                New Seta With {
                    .SetaCode = "ESETA",
                    .Name = "ESETA",
                    .FullName = "Energy Sector Education and Training Authority",
                    .Sector = "Energy",
                    .Description = "Electricity, gas, nuclear, renewable energy"
                },
                New Seta With {
                    .SetaCode = "FOODBEV",
                    .Name = "FoodBev SETA",
                    .FullName = "Food and Beverages Manufacturing SETA",
                    .Sector = "Food and Beverages Manufacturing",
                    .Description = "Food processing, beverages, and related industries"
                },
                New Seta With {
                    .SetaCode = "FPMSETA",
                    .Name = "FP&M SETA",
                    .FullName = "Fibre Processing and Manufacturing SETA",
                    .Sector = "Fibre, Paper, Pulp, Packaging and Furniture",
                    .Description = "Pulp and paper, furniture manufacturing, printing"
                },
                New Seta With {
                    .SetaCode = "HWSETA",
                    .Name = "HWSETA",
                    .FullName = "Health and Welfare Sector Education and Training Authority",
                    .Sector = "Health and Social Services",
                    .Description = "Healthcare, social development, community services"
                },
                New Seta With {
                    .SetaCode = "INSETA",
                    .Name = "INSETA",
                    .FullName = "Insurance Sector Education and Training Authority",
                    .Sector = "Insurance and Retirement Funds",
                    .Description = "Short-term insurance, long-term insurance, medical schemes, retirement funds"
                },
                New Seta With {
                    .SetaCode = "LGSETA",
                    .Name = "LGSETA",
                    .FullName = "Local Government Sector Education and Training Authority",
                    .Sector = "Local Government and Water Services",
                    .Description = "Municipal services, water and sanitation"
                },
                New Seta With {
                    .SetaCode = "MERSETA",
                    .Name = "merSETA",
                    .FullName = "Manufacturing, Engineering and Related Services SETA",
                    .Sector = "Manufacturing, Engineering and Related Services",
                    .Description = "Metal, auto, motor, engineering, and manufacturing"
                },
                New Seta With {
                    .SetaCode = "MICT",
                    .Name = "MICT SETA",
                    .FullName = "Media, Information and Communication Technologies SETA",
                    .Sector = "ICT, Media and Advertising",
                    .Description = "Information technology, telecommunications, advertising, media"
                },
                New Seta With {
                    .SetaCode = "MIMSSETA",
                    .Name = "MQA",
                    .FullName = "Mining Qualifications Authority",
                    .Sector = "Mining and Minerals",
                    .Description = "Mining, exploration, quarrying, beneficiation"
                },
                New Seta With {
                    .SetaCode = "PSETA",
                    .Name = "PSETA",
                    .FullName = "Public Service Sector Education and Training Authority",
                    .Sector = "Public Service and Administration",
                    .Description = "National and provincial government departments"
                },
                New Seta With {
                    .SetaCode = "SASSETA",
                    .Name = "SASSETA",
                    .FullName = "Safety and Security Sector Education and Training Authority",
                    .Sector = "Safety and Security",
                    .Description = "Private security, justice, correctional services, defense"
                },
                New Seta With {
                    .SetaCode = "SERVICES",
                    .Name = "Services SETA",
                    .FullName = "Services Sector Education and Training Authority",
                    .Sector = "Professional and Business Services",
                    .Description = "Business services, hospitality, tourism, gaming, conservation"
                },
                New Seta With {
                    .SetaCode = "TETA",
                    .Name = "TETA",
                    .FullName = "Transport Education and Training Authority",
                    .Sector = "Transport and Logistics",
                    .Description = "Road, rail, air, maritime transport, logistics, warehousing"
                },
                New Seta With {
                    .SetaCode = "THETA",
                    .Name = "THETA",
                    .FullName = "Tourism, Hospitality and Sport Education and Training Authority",
                    .Sector = "Tourism, Hospitality and Sport",
                    .Description = "Tourism, accommodation, food services, sport and recreation"
                },
                New Seta With {
                    .SetaCode = "WSETA",
                    .Name = "W&RSETA",
                    .FullName = "Wholesale and Retail Sector Education and Training Authority",
                    .Sector = "Wholesale and Retail",
                    .Description = "Wholesale, retail trade, motor trade"
                }
            }
        End Function
    End Class
End Namespace
