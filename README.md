# Duplicate Learner Detection System (IDP)

An AI-powered VB.NET solution for detecting and managing duplicate learner records across South African Sector Education and Training Authorities (SETAs).

## 📋 Overview

This system addresses the critical challenge of duplicate learner registrations across 21 SETAs in South Africa, which leads to:
- Inflated reporting and double funding
- Poor learner tracking
- Data inconsistencies and fragmented systems

### Key Features

- **🔍 Real-time Duplicate Detection**: Instant checks during learner registration
- **🤖 AI-Powered Matching**: ML.NET models with fuzzy matching algorithms
- **📊 Comprehensive Dashboard**: Visual insights into duplicates across SETAs
- **🔄 Batch Reconciliation**: Periodic scans across all SETA databases
- **✅ Resolution Workflow**: Review, confirm, merge, or reject flagged duplicates
- **📈 Reporting & Analytics**: Export summaries, funding impact, and audit logs
- **🔐 POPIA Compliant**: Full audit trail for governance and compliance

## 🏗️ Architecture

### Tech Stack

- **Backend**: VB.NET, ASP.NET Core Web API
- **Frontend**: React with Vite
- **Database**: SQL Server with Entity Framework Core
- **AI/ML**: ML.NET for similarity scoring
- **Matching Algorithms**: Levenshtein distance, Double Metaphone (phonetic matching)

### Solution Structure

```
IDP/
├── src/
│   ├── Domain/              # Core business entities and enums
│   ├── Application/         # Business logic, DTOs, and interfaces
│   ├── Infrastructure/      # Data access, external services, background jobs
│   └── Presentation/        # API controllers and configuration
└── client/                  # React frontend application
```

## 🚀 Getting Started

### Prerequisites

- Visual Studio 2022 or later
- .NET 6.0 SDK or later
- SQL Server 2019 or later
- Node.js 18+ and npm (for React client)

### Installation

1. **Clone the repository**
   ```powershell
   git clone https://github.com/SiphosethuLokwe/IDP.git
   cd IDP
   ```

2. **Configure Database**
   - Update connection string in `src/Presentation/appsettings.json`
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=IDPDb;Trusted_Connection=True;"
     }
   }
   ```

3. **Build the Solution**
   ```powershell
   dotnet restore
   dotnet build
   ```

4. **Run Migrations**
   ```powershell
   cd src/Infrastructure
   dotnet ef database update --startup-project ../Presentation
   ```

5. **Install Frontend Dependencies**
   ```powershell
   cd client
   npm install
   ```

6. **Run the Application**

   **Backend:**
   ```powershell
   cd src/Presentation
   dotnet run
   ```

   **Frontend:**
   ```powershell
   cd client
   npm run dev
   ```

   - API: `https://localhost:5001`
   - Frontend: `http://localhost:5173`

## 🔧 Configuration

### App Settings

Key configuration options in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=IDPDb;..."
  },
  "DuplicationDetection": {
    "MatchThreshold": 0.75,
    "AutoRejectThreshold": 0.30,
    "EnableMLMatching": true
  },
  "ExternalServices": {
    "CheckIdApiUrl": "https://api.checkid.za/verify",
    "ApiKey": "your-api-key-here"
  }
}
```

## 📊 Database Schema

### Core Tables

| Table | Purpose |
|-------|---------|
| **Learner** | Stores unique learner profiles (ID, name, DOB, contact info) |
| **SETA** | Defines 21 SETA authorities and their sectors |
| **Programme** | Stores qualifications and learnership details |
| **Contract** | Tracks learner funding contracts with SETAs |
| **DuplicationFlag** | Records detected duplicate learners |
| **DuplicationRule** | Configurable matching rules |
| **AuditLog** | Full audit trail for compliance |
| **ExternalVerification** | Results from external ID verification services |

### Relationships

- Learner ↔ Contract (1-to-many)
- SETA ↔ Contract (1-to-many)
- Programme ↔ Contract (1-to-many)
- DuplicationFlag ↔ Learner (many-to-many)

## 🤖 AI & Duplicate Detection

### Matching Strategies

1. **Exact Matching**: ID number, passport number
2. **Fuzzy Matching**: 
   - Levenshtein distance for name similarity
   - Double Metaphone for phonetic matching
3. **ML.NET Model**: Trained similarity scoring model
4. **Rule-Based Logic**: Configurable business rules

### Smart Detection Rules

| Scenario | Action |
|----------|--------|
| ID exists in another SETA with active funding for **same qualification** | ❌ BLOCK registration |
| ID exists but contract is **closed** | ✅ ALLOW, LINK records |
| ID exists with active funding for **different qualification** | ⚠️ ALLOW, FLAG for review |
| ID not found | ✅ REGISTER normally |

### Explainability

The system provides transparency by showing:
- Match score (0-100%)
- Matching factors (DOB match, surname similarity, phone match)
- Recommended action with reasoning

## 📈 Use Cases

### 1. New Learner Registration
- Real-time duplicate check during registration
- Instant feedback to administrator
- Block or flag based on match confidence

### 2. Batch Reconciliation
- Scheduled background jobs scan entire database
- Cross-SETA duplicate detection
- Generate reports for review

### 3. Admin Workflow
- Review flagged duplicates
- View side-by-side comparison
- Confirm, merge, link, or reject matches
- Add notes and reasoning

### 4. Reporting & Analytics
- Duplicate summary by SETA
- Funding impact analysis
- Audit trail exports
- Trend visualization

## 🔐 Security & Compliance

- **POPIA Compliance**: Full audit logging of all actions
- **Data Privacy**: Secure handling of personal information
- **Role-Based Access**: Configurable user permissions
- **Audit Trail**: Complete history of duplicate resolutions

## 🧪 Testing

```powershell
# Run all tests
dotnet test

# Run specific project tests
dotnet test src/Application.Tests/IDP.Application.Tests.vbproj
```

## 📦 Deployment

### Backend (IIS)
1. Publish the Presentation project
2. Configure IIS application pool (.NET CLR Version: No Managed Code)
3. Set environment variables
4. Configure SQL Server connection

### Frontend (Production Build)
```powershell
cd client
npm run build
```
Deploy the `dist/` folder to your web server.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 👥 Authors

- **Siphosethu Lokwe** - Initial work - [SiphosethuLokwe](https://github.com/SiphosethuLokwe)

## 🙏 Acknowledgments

- South African SETAs for domain knowledge
- ML.NET team for AI capabilities
- Open source community for fuzzy matching algorithms

## 📞 Support

For issues, questions, or contributions:
- Create an issue on GitHub
- Contact: [Your Email]
- Documentation: [Wiki Link]

## 🗺️ Roadmap

- [ ] Mobile application for field workers
- [ ] Integration with National Learner Records Database (NLRD)
- [ ] Advanced ML models with deep learning
- [ ] Real-time cross-SETA API integration
- [ ] Biometric verification support
- [ ] Multi-language support

---

**Built with ❤️ for transparent and efficient skills development in South Africa**
