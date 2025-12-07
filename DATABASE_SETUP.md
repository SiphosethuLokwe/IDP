# Database Setup Guide

This guide explains how to set up the SQL Server database for the Integrated Duplication Prevention (IDP) system.

## Prerequisites

1. **SQL Server** installed and running
   - SQL Server Express (free) or full version
   - Default instance listening on `localhost`
   - Windows Authentication enabled

2. **.NET 8.0 SDK** installed
   - Required for Entity Framework Core tools
   - Verify: `dotnet --version`

3. **EF Core Tools** installed
   - Run: `dotnet tool install --global dotnet-ef --version 8.0.22`
   - Or update: `dotnet tool update --global dotnet-ef --version 8.0.22`

## Connection String

The application uses SQL Server with Windows Authentication. The connection string is configured in `src/Presentation/appsettings.json` and `src/Presentation/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=IDP_DB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Connection String Components:
- **Server=localhost** - SQL Server instance (change if using remote server)
- **Database=IDP_DB** - Database name
- **Trusted_Connection=True** - Uses Windows Authentication
- **TrustServerCertificate=True** - Required for local development with SQL Server

## Simple Setup (Recommended)

**This is the standard approach - no workarounds needed!**

1. **Navigate to the source directory:**
   ```powershell
   cd src
   ```

2. **Apply the database migrations:**
   ```powershell
   dotnet ef database update --project IDP.Migrations --startup-project Presentation\IDP.Presentation.vbproj
   ```

That's it! This command will:
- Create the `IDP_DB` database if it doesn't exist
- Create all 8 application tables with proper schema
- Create all indexes and foreign keys
- Track the migration in `__EFMigrationsHistory`

### What Tables Are Created?

The migration creates these tables:
1. **Learners** - Learner records with ID numbers, names, contact info
2. **Setas** - 21 South African SETAs (Sector Education and Training Authorities)
3. **Programmes** - Training programmes and qualifications
4. **Contracts** - Funding contracts linking learners, SETAs, and programmes
5. **DuplicationFlags** - Detected duplicate learner records
6. **DuplicationRules** - Rules for detecting duplicates
7. **AuditLogs** - Audit trail of all database changes
8. **ExternalVerifications** - Results from external ID verification APIs

Additional tables created automatically:
- `__EFMigrationsHistory` - EF Core migration tracking
- Hangfire tables - Background job processing (created on first run)

## Verification

After running the migration, verify the setup:

### 1. Check Database Exists
```powershell
sqlcmd -S localhost -Q "SELECT name FROM sys.databases WHERE name = 'IDP_DB'"
```

### 2. List All Tables
```powershell
sqlcmd -S localhost -d IDP_DB -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME"
```

### 3. Check Migration History
```powershell
sqlcmd -S localhost -d IDP_DB -Q "SELECT MigrationId, ProductVersion FROM __EFMigrationsHistory"
```

Expected output:
```
MigrationId                    ProductVersion
20251207191508_InitialCreate  8.0.0
```

### 4. Run the Application
```powershell
cd src
dotnet run --project Presentation\IDP.Presentation.vbproj --urls="http://localhost:5000"
```

Application should start successfully on http://localhost:5000

## Adding New Migrations

When you modify entity models in the Domain project, create a new migration:

### 1. Generate Migration
```powershell
cd src
dotnet ef migrations add YourMigrationName --project IDP.Migrations --startup-project Presentation\IDP.Presentation.vbproj
```

### 2. Review Generated Code
Check `src/IDP.Migrations/Migrations/` for the new migration files.

### 3. Apply Migration
```powershell
dotnet ef database update --project IDP.Migrations --startup-project Presentation\IDP.Presentation.vbproj
```

## Recreating the Database

If you need to drop and recreate the database:

### Option 1: Drop and Recreate (Clean Slate)
```powershell
# Drop the database (forces single-user mode to close all connections)
sqlcmd -S localhost -Q "USE master; ALTER DATABASE IDP_DB SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE IDP_DB"

# Recreate with migrations
cd src
dotnet ef database update --project IDP.Migrations --startup-project Presentation\IDP.Presentation.vbproj
```

### Option 2: Remove All Migrations and Start Fresh
```powershell
# Remove all migrations
cd src\IDP.Migrations\Migrations
Remove-Item *.cs

# Drop database
sqlcmd -S localhost -Q "USE master; ALTER DATABASE IDP_DB SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE IDP_DB"

# Create new initial migration
cd ..\..\
dotnet ef migrations add InitialCreate --project IDP.Migrations --startup-project Presentation\IDP.Presentation.vbproj

# Apply migration
dotnet ef database update --project IDP.Migrations --startup-project Presentation\IDP.Presentation.vbproj
```

## Troubleshooting

### Issue: "Cannot drop database IDP_DB because it is currently in use"
**Solution:** Close all connections to the database:
```powershell
sqlcmd -S localhost -Q "USE master; ALTER DATABASE IDP_DB SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE IDP_DB"
```

### Issue: "There is already an object named 'TableName' in the database"
**Solution:** The database has tables from a previous creation method. Drop and recreate:
```powershell
sqlcmd -S localhost -Q "USE master; ALTER DATABASE IDP_DB SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE IDP_DB"
cd src
dotnet ef database update --project IDP.Migrations --startup-project Presentation\IDP.Presentation.vbproj
```

### Issue: "No executable found matching command 'dotnet-ef'"
**Solution:** Install EF Core tools:
```powershell
dotnet tool install --global dotnet-ef --version 8.0.22
```

### Issue: "Could not load assembly 'IDP.Migrations'"
**Solution:** Ensure the Presentation project references IDP.Migrations. Check `src/Presentation/IDP.Presentation.vbproj` contains:
```xml
<ProjectReference Include="..\IDP.Migrations\IDP.Migrations.csproj" />
```

### Issue: "Login failed for user"
**Solution:** 
1. Verify SQL Server is running
2. Check Windows Authentication is enabled
3. Ensure your Windows user has permission to create databases
4. Test connection: `sqlcmd -S localhost -Q "SELECT @@VERSION"`

### Issue: Decimal precision warnings
**Warning:** "No store type was specified for the decimal property 'ConfidenceScore'"

This is a warning, not an error. The database will use SQL Server's default precision (18, 2) for decimal columns. Tables are created successfully despite this warning.

## Why C# Migrations Project?

**VB.NET Limitation:** Entity Framework Core doesn't support generating migration code in VB.NET. While the DbContext can be written in VB.NET, EF Core only generates migration files in C#.

**Solution:** The `IDP.Migrations` C# project acts as a bridge:
- References the VB.NET Infrastructure and Domain projects
- Contains a design-time factory (`DesignTimeDbContextFactory.cs`)
- Stores all migration files in C#
- Configured in `DependencyInjection.vb` with: `sqlOptions.MigrationsAssembly("IDP.Migrations")`

This approach is standard for VB.NET projects using EF Core and requires no workarounds.

## Database Backup and Restore

### Backup
```powershell
sqlcmd -S localhost -Q "BACKUP DATABASE IDP_DB TO DISK = 'C:\Backup\IDP_DB.bak' WITH FORMAT"
```

### Restore
```powershell
sqlcmd -S localhost -Q "RESTORE DATABASE IDP_DB FROM DISK = 'C:\Backup\IDP_DB.bak' WITH REPLACE"
```

## Additional Resources

- [Entity Framework Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [SQL Server Connection Strings](https://www.connectionstrings.com/sql-server/)
- [Hangfire Documentation](https://docs.hangfire.io/)

---

**Last Updated:** December 7, 2024  
**EF Core Version:** 8.0.0  
**.NET Version:** 8.0
