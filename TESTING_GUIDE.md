# IDP API Testing Guide

## Overview
This guide provides instructions for testing the IDP (Individual Development Plan) API endpoints with the seeded test data.

## Starting the API

### Option 1: Command Line
```powershell
cd C:\Users\doria\IDP
dotnet run --project src\Presentation\IDP.Presentation.vbproj --urls="http://localhost:5000"
```

### Option 2: Visual Studio
1. Open `IDP.sln` in Visual Studio
2. Set `IDP.Presentation` as the startup project
3. Press F5 to run

The API will be available at: **http://localhost:5000**

## Swagger UI
Access the interactive API documentation at: **http://localhost:5000/swagger**

## Seeded Test Data

The database is automatically seeded on startup with the following data:

### SETAs (21 Total)
All 21 South African SETAs are seeded with:
- AGRISETA, BANKSETA, CATHSSETA, CHIETA, CETA
- ETDPSETA, EWSETA, FASSET, FIETA, FOODBEV
- HWSETA, INSETA, LGSETA, MAPPP, MERSETA
- MICT, PSETA, SASSETA, SERVICES, TETA, WSETA

Each SETA includes:
- SetaCode (unique identifier)
- Full name and sector
- Contact email, phone, website
- Established date
- Description

### Programmes (5 Total)
| Code | Title | SETA | NQF Level | Duration |
|------|-------|------|-----------|----------|
| Q0001 | IT Systems Development | MICT | 5 | 12 months |
| Q0002 | IT Technical Support | MICT | 4 | 12 months |
| Q0003 | Engineering Diploma | MERSETA | 6 | 36 months |
| Q0004 | Nursing Assistant | HWSETA | 4 | 12 months |
| Q0005 | Project Management | Generic | 5 | 6 months |

### Learners (5 Total)
| ID Number | Name | SETA |
|-----------|------|------|
| 9501015800081 | Thabo Mbeki | MICT |
| 9802155600082 | Nomsa Dlamini | HWSETA |
| 0005125800083 | Sipho Nkosi | MERSETA |
| 9707085600084 | Lerato Mokoena | MICT |
| 9909215800085 | Mandla Khumalo | SERVICES |

### Contracts (2 Total)
| Contract Number | Learner | Programme | SETA | Funding | Status |
|----------------|---------|-----------|------|---------|--------|
| CNT-2024-001 | Thabo Mbeki | IT Systems Development | MICT | R35,000 | Active |
| CNT-2024-002 | Nomsa Dlamini | Nursing Assistant | HWSETA | R28,000 | Active |

## Testing API Endpoints

### 1. SETAs Endpoints

#### Get All SETAs
```powershell
# PowerShell
$setas = (curl http://localhost:5000/api/setas).Content | ConvertFrom-Json
Write-Host "Total SETAs: $($setas.Count)"
$setas | Select-Object SetaCode, Name, Sector | Format-Table -AutoSize
```

```bash
# Bash/Linux
curl http://localhost:5000/api/setas | jq '.[] | {setaCode, name, sector}'
```

**Expected Result:** 21 SETAs

#### Get SETA by SetaCode
```powershell
# PowerShell
$seta = (curl "http://localhost:5000/api/setas/by-code/MICT").Content | ConvertFrom-Json
$seta | Format-List
```

**Expected Result:** Full details of MICT SETA

#### Get Active SETAs
```powershell
# PowerShell
$activeSetas = (curl http://localhost:5000/api/setas/active).Content | ConvertFrom-Json
Write-Host "Active SETAs: $($activeSetas.Count)"
```

**Expected Result:** 21 active SETAs

### 2. Learners Endpoints

#### Get All Learners
```powershell
# PowerShell
$learners = (curl http://localhost:5000/api/learners).Content | ConvertFrom-Json
Write-Host "Total Learners: $($learners.Count)"
$learners | Select-Object FirstName, LastName, IdNumber, SetaCode | Format-Table -AutoSize
```

**Expected Result:** 5 learners

#### Get Learner by ID Number
```powershell
# PowerShell
$learner = (curl "http://localhost:5000/api/learners/by-idnumber/9501015800081").Content | ConvertFrom-Json
$learner | Format-List
```

**Expected Result:** Thabo Mbeki's details

#### Create New Learner (Duplicate Detection Test)
```powershell
# PowerShell - Create a similar learner to test duplicate detection
$body = @{
    IdNumber = "9501015800089"  # Similar to Thabo's
    FirstName = "Thabo"
    LastName = "Mbeki"
    DateOfBirth = "1995-01-01"
    Email = "thabo.mbeki2@example.com"
    PhoneNumber = "0821234567"
    SetaCode = "MICT"
} | ConvertTo-Json

curl -Method POST -Uri "http://localhost:5000/api/learners" `
     -ContentType "application/json" `
     -Body $body
```

**Expected Result:** Learner created, duplicate detection should flag potential match

### 3. Duplicate Detection Endpoints

#### Get All Duplication Flags
```powershell
# PowerShell
$duplicates = (curl http://localhost:5000/api/duplications/flags).Content | ConvertFrom-Json
$duplicates | Select-Object Status, MatchType, ConfidenceScore | Format-Table -AutoSize
```

#### Check for Duplicates (Manual)
```powershell
# PowerShell
$checkData = @{
    IdNumber = "9501015800081"
} | ConvertTo-Json

$result = curl -Method POST -Uri "http://localhost:5000/api/duplications/check" `
               -ContentType "application/json" `
               -Body $checkData
$result.Content | ConvertFrom-Json | Format-List
```

**Expected Result:** Should detect if similar learner exists

### 4. Database Verification

You can also verify the seeded data directly in SQL Server:

```sql
-- Check SETAs count
SELECT COUNT(*) AS TotalSetas FROM Setas;
-- Expected: 21

-- Check Learners count
SELECT COUNT(*) AS TotalLearners FROM Learners;
-- Expected: 5

-- Check Programmes count
SELECT COUNT(*) AS TotalProgrammes FROM Programmes;
-- Expected: 5

-- Check Contracts count
SELECT COUNT(*) AS TotalContracts FROM Contracts;
-- Expected: 2

-- View all SETAs
SELECT SetaCode, Name, Sector FROM Setas ORDER BY SetaCode;

-- View all Learners with their SETAs
SELECT l.IdNumber, l.FirstName, l.LastName, l.SetaCode 
FROM Learners l 
ORDER BY l.FirstName;

-- View all Contracts with details
SELECT 
    c.ContractNumber,
    l.FirstName + ' ' + l.LastName AS Learner,
    p.Title AS Programme,
    s.Name AS SETA,
    c.FundingAmount,
    c.Status
FROM Contracts c
JOIN Learners l ON c.LearnerId = l.Id
JOIN Programmes p ON c.ProgrammeId = p.Id
JOIN Setas s ON c.SetaId = s.Id;
```

## Testing Duplicate Detection

### Scenario 1: Exact ID Number Match
Try to create a learner with ID number `9501015800081` (Thabo Mbeki's ID).

**Expected:** System should detect exact duplicate and flag it.

### Scenario 2: Similar Name and Demographics
Create a learner with:
- Different ID: `9501025800082`
- Same name: `Thabo Mbeki`
- Similar date of birth
- Same SETA: `MICT`

**Expected:** System should flag as potential duplicate based on fuzzy matching.

### Scenario 3: Different Learner
Create a learner with completely different details:
- ID: `8801015800083`
- Name: `Jane Smith`
- SETA: `BANKSETA`

**Expected:** No duplicate flags should be raised.

## Available Controllers

1. **SetasController** (`/api/setas`)
   - GET: Get all SETAs
   - GET: Get active SETAs
   - GET: Get by ID
   - GET: Get by SETA code

2. **LearnersController** (`/api/learners`)
   - GET: Get all learners
   - GET: Get by ID
   - GET: Get by ID number
   - POST: Create new learner
   - PUT: Update learner
   - DELETE: Delete learner

3. **DuplicationsController** (`/api/duplications`)
   - GET: Get all duplication flags
   - POST: Manual duplicate check
   - PUT: Resolve duplication flag

4. **ContractsController** (`/api/contracts`)
   - Various contract management endpoints

## Testing with Postman

1. Import the Swagger JSON from `http://localhost:5000/swagger/v1/swagger.json`
2. Create a new environment with:
   - `baseUrl`: `http://localhost:5000`
3. Test each endpoint using the examples above

## Troubleshooting

### Server Won't Start
```powershell
# Check if port 5000 is in use
netstat -ano | findstr :5000

# Kill the process if needed
Stop-Process -Id <PID> -Force
```

### Database Not Seeded
The seeder checks if SETAs already exist. To force re-seeding:
1. Delete all data: `DELETE FROM Setas; DELETE FROM Learners; etc.`
2. Restart the application

### 500 Internal Server Error
Check the application logs in the console where the API is running for detailed error messages.

## Next Steps

1. ✅ Test all SETA endpoints
2. ✅ Test all Learner endpoints
3. ⏳ Test duplicate detection with various scenarios
4. ⏳ Test contract management endpoints
5. ⏳ Test fuzzy matching algorithms
6. ⏳ Re-enable and test ML.NET prediction features
7. ⏳ Performance testing with larger datasets

## Notes

- The database seeder runs automatically on startup
- If data already exists (21 SETAs found), seeding is skipped
- All test learners have valid South African ID numbers
- Duplicate detection is currently working without ML features (to be re-enabled later)
