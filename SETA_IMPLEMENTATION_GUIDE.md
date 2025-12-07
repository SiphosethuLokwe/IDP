# SETA-Specific Implementation Guide

## Understanding SETAs in South Africa

### What are SETAs?

**Sector Education and Training Authorities (SETAs)** are statutory bodies established under the Skills Development Act of 1998. South Africa has **21 SETAs**, each responsible for managing skills development, learnerships, apprenticeships, and training within specific industry sectors.

### The 21 SETAs

1. **AGRISETA** - Agriculture and Related Services
2. **BANKSETA** - Banking, Financial Services
3. **CHIETA** - Chemical Industries
4. **CTFL** - Clothing, Textiles, Footwear, Leather
5. **CETA** - Construction and Built Environment
6. **ETDP SETA** - Education, Training and Development
7. **ESETA** - Energy Sector
8. **FoodBev SETA** - Food and Beverages Manufacturing
9. **FP&M SETA** - Fibre Processing and Manufacturing
10. **HWSETA** - Health and Welfare
11. **INSETA** - Insurance and Retirement Funds
12. **LGSETA** - Local Government and Water
13. **merSETA** - Manufacturing, Engineering
14. **MICT SETA** - Media, ICT, Advertising
15. **MQA** - Mining Qualifications Authority
16. **PSETA** - Public Service
17. **SASSETA** - Safety and Security
18. **Services SETA** - Business Services, Hospitality
19. **TETA** - Transport and Logistics
20. **THETA** - Tourism, Hospitality, Sport
21. **W&RSETA** - Wholesale and Retail

## The Duplicate Learner Problem

### Why It Matters

#### 1. **Financial Impact**
- Each SETA receives funding from the Skills Development Levy
- A learner registered in multiple SETAs means **double or triple funding**
- Estimated loss: **R500 million+ annually** to fraud and duplicates

#### 2. **Reporting Accuracy**
- Government reports on skills development rely on accurate learner counts
- Duplicates inflate success metrics and hide actual training gaps
- Poor policy decisions result from inaccurate data

#### 3. **Learner Tracking**
- Employers struggle to verify learner qualifications
- SETAs can't track learner progression across sectors
- Career mobility hindered by fragmented records

### Real-World Scenarios

#### Scenario 1: The Career Changer
**Situation**: 
- Sipho completed a learnership with **BANKSETA** (Banking) in 2022
- In 2024, he wants to switch careers to IT
- He applies for a new learnership with **MICT SETA**

**Current Problem**: 
- No automated way to link his records
- MICT SETA doesn't know about his BANKSETA training
- Risk of duplicate ID number flags blocking legitimate career change

**Our Solution**:
```
RULE: IF learner ID exists but contract is CLOSED → ALLOW new registration, LINK records
Result: ✅ Sipho is allowed to register, and his records are linked for comprehensive tracking
```

#### Scenario 2: The Fraudster
**Situation**:
- Thandi is currently funded by **CETA** (Construction) for NQF Level 4 Building
- She uses the same ID to apply for the same qualification with **MERSETA**

**Current Problem**:
- No cross-SETA validation
- She receives double funding for the same qualification
- Government pays twice for the same training

**Our Solution**:
```
RULE: IF learner ID exists in another SETA with active funding for SAME qualification → BLOCK
Result: ❌ Registration blocked with clear explanation about existing funding
```

#### Scenario 3: The Multi-Skiller
**Situation**:
- Lerato is funded by **HWSETA** (Healthcare) for Nursing Assistant
- She wants to study part-time IT skills with **MICT SETA** (different qualification)

**Current Problem**:
- Unclear if this is allowed
- Manual review takes weeks
- Training opportunity may be lost

**Our Solution**:
```
RULE: IF learner has active funding in another SETA for DIFFERENT qualification → ALLOW with FLAG
Result: ⚠️ Allowed immediately but flagged for admin review within 48 hours
```

#### Scenario 4: The Typo Victim
**Situation**:
- Nomsa's ID: 9012155678089
- Due to data entry error, she's in the system as: 9012155678**0**89 and 9012155678**0**99
- She can't register because the system flags her as a duplicate

**Current Problem**:
- Exact ID matching misses typos
- Manual intervention required
- Legitimate learner blocked from training

**Our Solution**:
```
AI-Powered Fuzzy Matching:
- Levenshtein distance detects similar IDs (1 character difference)
- Flagged as "Possible Data Entry Error" with 75% confidence
- Admin can merge records with audit trail
```

## Smart Duplicate Detection Rules (Implementation)

### Rule Engine Logic

```vb
' RULE 1: BLOCK - Same Qualification, Active in Another SETA
IF existingContract.IsActive AND 
   existingContract.SetaId <> newSetaId AND
   existingContract.Qualification = newQualification THEN
   RETURN "BLOCK - Double funding not allowed"
END IF

' RULE 2: ALLOW & LINK - Closed Contract in Another SETA
IF existingContract.IsClosed() AND
   existingContract.SetaId <> newSetaId THEN
   RETURN "ALLOW & LINK - Career progression tracked"
END IF

' RULE 3: ALLOW & FLAG - Different Qualification, Active
IF existingContract.IsActive AND
   existingContract.SetaId <> newSetaId AND
   existingContract.Qualification <> newQualification THEN
   RETURN "ALLOW & FLAG - Multi-skilling requires review"
END IF

' RULE 4: ALLOW - No Existing Contracts
IF NOT existingContracts.Any() THEN
   RETURN "ALLOW - New learner registration"
END IF
```

## Real-World Use Cases

### Use Case 1: New Learner Registration Portal

**User Story**: As a SETA administrator, I want to register a new learner and immediately know if they're already registered elsewhere.

**Flow**:
1. Admin enters learner details (ID, name, DOB)
2. System checks across all 21 SETAs in real-time
3. Results displayed within 2 seconds:
   - ✅ **Green**: No duplicates, proceed
   - ⚠️ **Yellow**: Possible match, review recommended
   - ❌ **Red**: Active duplicate found, blocked

**API Endpoint**:
```
POST /api/learners/validate
{
  "idNumber": "9012155678089",
  "firstName": "Sipho",
  "lastName": "Nkosi",
  "dateOfBirth": "1990-12-15",
  "targetSetaId": "guid",
  "targetProgrammeId": "guid"
}

Response:
{
  "isAllowed": false,
  "action": "BLOCK",
  "reason": "Learner has active funding in BANKSETA for same qualification",
  "matchConfidence": 100,
  "existingContracts": [...]
}
```

### Use Case 2: Monthly Reconciliation Report

**User Story**: As a SETA manager, I need a monthly report showing all potential duplicate learners across SETAs for investigation.

**Report Contents**:
- High-confidence duplicates (90%+) - Immediate action needed
- Medium-confidence duplicates (70-89%) - Review within 30 days
- Low-confidence duplicates (50-69%) - Monitor for patterns

**Batch Process**:
```powershell
# Scheduled job runs on 1st of every month
dotnet run --project BackgroundJobs -- reconciliation --all-setas --output-report
```

### Use Case 3: Fraud Detection Dashboard

**User Story**: As a compliance officer, I need to identify potential funding fraud patterns across SETAs.

**Dashboard Metrics**:
- **Red Alerts**: Same ID, same qualification, multiple SETAs (urgent)
- **Orange Alerts**: Overlapping contract dates
- **Yellow Alerts**: Same learner, 3+ active contracts
- **Trend Analysis**: Fraudulent patterns by region, provider, qualification

**Real-Time Monitoring**:
```
Alert Example:
🚨 HIGH SEVERITY FRAUD ALERT
Learner: ID 8505206789012
Issue: Receiving funding for "Plumbing NQF4" from BOTH:
  - CETA (Construction) - Active since 2024-01-15
  - merSETA (Manufacturing) - Active since 2024-03-10
Estimated Loss: R45,000
Action Required: Investigate within 24 hours
```

### Use Case 4: Learner Career Portal

**User Story**: As a learner, I want to see my complete training history across all SETAs to build my portfolio.

**Features**:
- Timeline view of all completed/active learnerships
- Digital certificates from multiple SETAs
- Skills matrix showing NQF progression
- Recommendations for next career step

**Benefits**:
- Empowers learners with their data
- Encourages career progression
- Validates cross-sector skills

## Technical Implementation

### Database Seeding

```vb
' Seed all 21 SETAs on first run
Public Async Function SeedSetasAsync() As Task
    Dim setas = Seta.GetAllSetas()
    
    For Each seta In setas
        If Not Await _context.Setas.AnyAsync(Function(s) s.SetaCode = seta.SetaCode) Then
            _context.Setas.Add(seta)
        End If
    Next
    
    Await _context.SaveChangesAsync()
End Function
```

### Cross-SETA Validation Service

The `CrossSetaValidationService` implements all 4 rules from the specification:

```vb
Dim validationResult = Await _crossSetaService.ValidateNewRegistrationAsync(
    learnerId, 
    targetSetaId, 
    targetProgrammeId
)

If Not validationResult.IsAllowed Then
    Return BadRequest(validationResult.Reason)
End If
```

### Fraud Detection

```vb
' Detect potential fraud
Dim fraudAlerts = Await _crossSetaService.DetectCrossSetaFraudAsync(learnerId)

If fraudAlerts.Any(Function(a) a.Severity = "HIGH") Then
    ' Send immediate alert to compliance team
    Await _notificationService.SendFraudAlertAsync(fraudAlerts)
End If
```

## Integration Points

### 1. National Learner Records Database (NLRD)
- Push validated learner records to NLRD
- Pull verification data for cross-validation

### 2. Home Affairs (DHA) Integration
- Verify ID numbers against DHA database
- Detect deceased persons, invalid IDs

### 3. SARS Skills Levy System
- Verify employer contributions
- Match levy payments to learner funding

### 4. SAQA (South African Qualifications Authority)
- Validate qualification codes
- Verify NQF levels and credits

## Compliance & Governance

### POPIA Compliance
- **Consent Management**: Learners consent to cross-SETA data sharing
- **Data Minimization**: Only share necessary fields
- **Audit Trail**: Full logging of all data access
- **Right to Access**: Learners can view their complete records

### Audit Requirements
Every duplicate resolution logged with:
- Who made the decision
- When it was made
- What evidence was reviewed
- Final outcome and reasoning

### Reporting to DHET (Department of Higher Education and Training)
- Monthly statistical reports
- Quarterly fraud reports
- Annual skills development impact

## Performance Considerations

### Scalability
- **Current Load**: 50,000 learners per SETA × 21 SETAs = **1.05 million learners**
- **Expected Growth**: 15% annually
- **Query Performance**: Sub-200ms for duplicate checks using indexed searches
- **Batch Processing**: 100,000 comparisons per hour

### Optimization Strategies
1. **Indexed Fields**: ID number, phone, email
2. **Caching**: SETA data, qualification codes
3. **Async Processing**: Background jobs for bulk reconciliation
4. **Database Partitioning**: By SETA for faster queries

## Future Enhancements

### Phase 2 (Q2 2026)
- [ ] Biometric verification (fingerprint, facial recognition)
- [ ] Mobile app for learners
- [ ] SMS notifications for duplicate alerts
- [ ] Integration with NSFAS (student funding)

### Phase 3 (Q4 2026)
- [ ] Machine learning model improvements
- [ ] Predictive analytics for fraud detection
- [ ] Real-time API for employer verification
- [ ] Blockchain for immutable learner records

## Getting Started with SETA Features

### 1. Seed Database
```powershell
cd src/Infrastructure
dotnet ef migrations add AddSetaEntities
dotnet ef database update
```

### 2. Run Seeder
```vb
' In Program.vb startup
Await SeedDatabaseAsync(app.Services)
```

### 3. Test Cross-SETA Validation
```powershell
# Test endpoint
curl -X POST https://localhost:5001/api/learners/validate ^
  -H "Content-Type: application/json" ^
  -d "{\"idNumber\":\"9012155678089\",\"targetSetaId\":\"guid\"}"
```

## Support & Documentation

- **Wiki**: [Internal Wiki Link]
- **API Docs**: `/swagger`
- **Training Videos**: [SharePoint Link]
- **Support Email**: idp-support@seta.gov.za

---

**Last Updated**: December 2025  
**Version**: 1.0  
**Author**: Development Team
