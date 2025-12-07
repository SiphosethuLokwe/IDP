# Quick Start Guide - SETA Duplicate Detection System

## 🚀 5-Minute Setup

### Step 1: Restore Dependencies
```powershell
cd c:\Users\doria\IDP
dotnet restore
cd client
npm install
```

### Step 2: Update Connection String
Edit `src/Presentation/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=IDP_Dev;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Step 3: Create Database & Run Migrations
```powershell
cd src/Infrastructure
dotnet ef migrations add AddSetaEntities --startup-project ../Presentation
dotnet ef database update --startup-project ../Presentation
```

### Step 4: Run Backend
```powershell
cd src/Presentation
dotnet run
```
Backend will start at: `https://localhost:5001`

### Step 5: Run Frontend
```powershell
cd client
npm run dev
```
Frontend will start at: `http://localhost:5173`

---

## 📋 Testing Cross-SETA Validation

### Test 1: Check if SETAs are seeded
```powershell
curl https://localhost:5001/api/setas/active
```
Expected: List of 21 SETAs

### Test 2: Validate a new learner registration
```powershell
curl -X POST https://localhost:5001/api/contracts/validate `
  -H "Content-Type: application/json" `
  -d '{
    "learnerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "setaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "programmeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  }'
```

### Test 3: Check for duplicates
```powershell
curl -X POST https://localhost:5001/api/duplications/check `
  -H "Content-Type: application/json" `
  -d '{
    "idNumber": "9012155678089",
    "firstName": "Sipho",
    "lastName": "Nkosi",
    "dateOfBirth": "1990-12-15"
  }'
```

---

## 🗄️ Database Schema Overview

### Core Tables
```
Learner (learner profile)
  └─> Contract (funding agreement)
        ├─> Seta (21 training authorities)
        └─> Programme (qualifications)

DuplicationFlag (detected duplicates)
  ├─> Learner (original)
  └─> DuplicateLearner (match found)
```

---

## 🎯 Key API Endpoints

### Learners
- `POST /api/learners` - Register new learner
- `GET /api/learners/{id}` - Get learner details
- `GET /api/learners` - List all learners

### Duplications
- `POST /api/duplications/check` - Check for duplicates
- `GET /api/duplications/pending` - Get pending flags
- `PUT /api/duplications/{id}/resolve` - Resolve duplicate

### SETAs (NEW!)
- `GET /api/setas` - List all 21 SETAs
- `GET /api/setas/code/BANKSETA` - Get specific SETA

### Contracts (NEW!)
- `POST /api/contracts/validate` - **Cross-SETA validation**
- `POST /api/contracts` - Create contract (auto-validates)
- `GET /api/contracts/learner/{id}/history` - Full training history
- `GET /api/contracts/learner/{id}/fraud-check` - Detect fraud

---

## 🔍 Understanding the 4 Validation Rules

### Rule 1: BLOCK Double Funding ❌
```
Scenario: Learner has ACTIVE contract for SAME qualification in another SETA
Action: BLOCK registration
Example: Sipho is funded by CETA for "Plumbing NQF4"
         He tries to register with merSETA for "Plumbing NQF4"
Result: ❌ BLOCKED - "Double funding not allowed"
```

### Rule 2: ALLOW & LINK Career Change ✅🔗
```
Scenario: Learner has CLOSED contract in another SETA
Action: ALLOW new registration, LINK records
Example: Thandi completed Banking learnership with BANKSETA (closed)
         She now wants to study IT with MICT SETA
Result: ✅ ALLOWED & records linked for career tracking
```

### Rule 3: ALLOW & FLAG Multi-Skilling ⚠️
```
Scenario: Learner has ACTIVE contract for DIFFERENT qualification
Action: ALLOW registration, FLAG for review
Example: Lerato is studying Nursing with HWSETA
         She wants part-time IT course with MICT SETA
Result: ⚠️ ALLOWED but flagged for admin review within 48hrs
```

### Rule 4: ALLOW New Learner ✅
```
Scenario: No existing contracts found
Action: ALLOW registration normally
Example: New learner with no training history
Result: ✅ ALLOWED - Welcome to the system!
```

---

## 🧪 Sample Test Data

### Create a SETA (if not seeded)
```powershell
curl -X POST https://localhost:5001/api/setas `
  -H "Content-Type: application/json" `
  -d '{
    "setaCode": "BANKSETA",
    "name": "BANKSETA",
    "fullName": "Banking Sector Education and Training Authority",
    "sector": "Banking and Financial Services",
    "isActive": true
  }'
```

### Create a Programme
```powershell
curl -X POST https://localhost:5001/api/programmes `
  -H "Content-Type: application/json" `
  -d '{
    "qualificationCode": "49001",
    "title": "National Certificate: Banking Services",
    "nqfLevel": 4,
    "credits": 120,
    "programmeType": "Learnership",
    "duration": 12,
    "setaId": "<BANKSETA_GUID>",
    "isActive": true
  }'
```

### Register a Learner
```powershell
curl -X POST https://localhost:5001/api/learners `
  -H "Content-Type: application/json" `
  -d '{
    "idNumber": "9012155678089",
    "firstName": "Sipho",
    "lastName": "Nkosi",
    "dateOfBirth": "1990-12-15",
    "phoneNumber": "0821234567",
    "email": "sipho.nkosi@example.com",
    "setaCode": "BANKSETA"
  }'
```

### Create a Contract
```powershell
curl -X POST https://localhost:5001/api/contracts `
  -H "Content-Type: application/json" `
  -d '{
    "contractNumber": "BANK-2025-001",
    "learnerId": "<LEARNER_GUID>",
    "setaId": "<SETA_GUID>",
    "programmeId": "<PROGRAMME_GUID>",
    "startDate": "2025-01-15",
    "endDate": "2026-01-14",
    "status": 1,
    "fundingAmount": 45000
  }'
```

---

## 🐛 Troubleshooting

### Issue: "SETAs table is empty"
**Solution**: Run the seeder in Program.vb or manually POST all 21 SETAs

### Issue: "Cross-SETA validation returns null"
**Solution**: Ensure Contracts table has Include() for Seta and Programme navigation properties

### Issue: "Migration fails"
**Solution**: 
```powershell
dotnet ef database drop --force --startup-project ../Presentation
dotnet ef database update --startup-project ../Presentation
```

### Issue: "Frontend can't connect to API"
**Solution**: Check CORS settings in Program.vb and ensure backend is running on port 5001

---

## 📚 Learn More

- **Full Documentation**: See `README.md`
- **SETA Details**: See `SETA_IMPLEMENTATION_GUIDE.md`
- **Implementation Summary**: See `IMPLEMENTATION_SUMMARY.md`
- **API Docs**: Visit `https://localhost:5001/swagger` when backend is running

---

## 🎯 Common Development Tasks

### Add a new SETA
1. Navigate to `src/Domain/Entities/Seta.vb`
2. Add to `GetAllSetas()` function
3. Run migration

### Add a new validation rule
1. Open `src/Application/Services/CrossSetaValidationService.vb`
2. Add logic to `ValidateNewRegistrationAsync()`
3. Update tests

### Add fraud detection pattern
1. Open `src/Application/Services/CrossSetaValidationService.vb`
2. Add logic to `DetectCrossSetaFraudAsync()`
3. Define alert type and severity

---

## ✅ Verification Checklist

- [ ] Database created and migrations applied
- [ ] 21 SETAs seeded in database
- [ ] Backend running on port 5001
- [ ] Frontend running on port 5173
- [ ] Swagger UI accessible
- [ ] Can create learner via API
- [ ] Can validate contract via API
- [ ] Cross-SETA validation working

---

## 🆘 Getting Help

1. Check error logs in `src/Presentation/bin/Debug/net6.0/logs/`
2. View Swagger API docs at `/swagger`
3. Check database with SQL Server Management Studio
4. Review `IMPLEMENTATION_SUMMARY.md` for architecture

---

**Happy Coding! 🚀**
