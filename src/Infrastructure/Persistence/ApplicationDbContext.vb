Imports Microsoft.EntityFrameworkCore
Imports IDP.Domain.Entities

Namespace Persistence
    Public Class ApplicationDbContext
        Inherits DbContext
        
        Public Sub New(options As DbContextOptions(Of ApplicationDbContext))
            MyBase.New(options)
        End Sub
        
        Public Property Learners As DbSet(Of Learner)
        Public Property DuplicationFlags As DbSet(Of DuplicationFlag)
        Public Property DuplicationRules As DbSet(Of DuplicationRule)
        Public Property ExternalVerifications As DbSet(Of ExternalVerification)
        Public Property AuditLogs As DbSet(Of AuditLog)
        Public Property Setas As DbSet(Of Seta)
        Public Property Programmes As DbSet(Of Programme)
        Public Property Contracts As DbSet(Of Contract)
        
        Protected Overrides Sub OnModelCreating(modelBuilder As ModelBuilder)
            MyBase.OnModelCreating(modelBuilder)
            
            ' Learner configuration
            modelBuilder.Entity(Of Learner)(Sub(entity)
                entity.HasKey(Function(e) e.Id)
                entity.Property(Function(e) e.IdNumber).HasMaxLength(20).IsRequired()
                entity.Property(Function(e) e.FirstName).HasMaxLength(100).IsRequired()
                entity.Property(Function(e) e.LastName).HasMaxLength(100).IsRequired()
                entity.Property(Function(e) e.Email).HasMaxLength(200)
                entity.Property(Function(e) e.PhoneNumber).HasMaxLength(20)
                entity.Property(Function(e) e.PassportNumber).HasMaxLength(50)
                entity.Property(Function(e) e.AlternativeIdNumber).HasMaxLength(20)
                entity.Property(Function(e) e.SetaCode).HasMaxLength(50)
                entity.Property(Function(e) e.OrganizationId).HasMaxLength(100)
                entity.HasIndex(Function(e) e.IdNumber).IsUnique()
                entity.HasIndex(Function(e) e.Email)
                entity.HasIndex(Function(e) e.PhoneNumber)
            End Sub)
            
            ' DuplicationFlag configuration
            modelBuilder.Entity(Of DuplicationFlag)(Sub(entity)
                entity.HasKey(Function(e) e.Id)
                entity.Property(Function(e) e.MatchDetails).HasColumnType("nvarchar(max)")
                entity.Property(Function(e) e.Notes).HasColumnType("nvarchar(max)")
                entity.Property(Function(e) e.ReviewedBy).HasMaxLength(100)
                entity.HasOne(Function(e) e.Learner) _
                      .WithMany(Function(e) e.DuplicationFlags) _
                      .HasForeignKey(Function(e) e.LearnerId) _
                      .OnDelete(DeleteBehavior.Restrict)
                entity.HasOne(Function(e) e.DuplicateLearner) _
                      .WithMany() _
                      .HasForeignKey(Function(e) e.DuplicateLearnerId) _
                      .OnDelete(DeleteBehavior.Restrict)
                entity.HasIndex(Function(e) e.LearnerId)
                entity.HasIndex(Function(e) e.Status)
            End Sub)
            
            ' DuplicationRule configuration
            modelBuilder.Entity(Of DuplicationRule)(Sub(entity)
                entity.HasKey(Function(e) e.Id)
                entity.Property(Function(e) e.RuleName).HasMaxLength(200).IsRequired()
                entity.Property(Function(e) e.RuleDescription).HasMaxLength(500)
                entity.Property(Function(e) e.RuleJson).HasColumnType("nvarchar(max)").IsRequired()
                entity.HasIndex(Function(e) e.IsActive)
            End Sub)
            
            ' ExternalVerification configuration
            modelBuilder.Entity(Of ExternalVerification)(Sub(entity)
                entity.HasKey(Function(e) e.Id)
                entity.Property(Function(e) e.VerificationProvider).HasMaxLength(100)
                entity.Property(Function(e) e.VerificationStatus).HasMaxLength(50)
                entity.Property(Function(e) e.RequestPayload).HasColumnType("nvarchar(max)")
                entity.Property(Function(e) e.ResponsePayload).HasColumnType("nvarchar(max)")
                entity.Property(Function(e) e.ErrorMessage).HasMaxLength(500)
                entity.HasOne(Function(e) e.Learner) _
                      .WithMany() _
                      .HasForeignKey(Function(e) e.LearnerId) _
                      .OnDelete(DeleteBehavior.Cascade)
                entity.HasIndex(Function(e) e.LearnerId)
            End Sub)
            
            ' AuditLog configuration
            modelBuilder.Entity(Of AuditLog)(Sub(entity)
                entity.HasKey(Function(e) e.Id)
                entity.Property(Function(e) e.EntityName).HasMaxLength(100).IsRequired()
                entity.Property(Function(e) e.Action).HasMaxLength(50).IsRequired()
                entity.Property(Function(e) e.PerformedBy).HasMaxLength(100)
                entity.Property(Function(e) e.IpAddress).HasMaxLength(50)
                entity.Property(Function(e) e.OldValues).HasColumnType("nvarchar(max)")
                entity.Property(Function(e) e.NewValues).HasColumnType("nvarchar(max)")
                entity.HasIndex(Function(e) e.EntityId)
                entity.HasIndex(Function(e) e.PerformedAt)
            End Sub)
            
            ' SETA configuration
            modelBuilder.Entity(Of Seta)(Sub(entity)
                entity.HasKey(Function(e) e.Id)
                entity.Property(Function(e) e.SetaCode).HasMaxLength(20).IsRequired()
                entity.Property(Function(e) e.Name).HasMaxLength(100).IsRequired()
                entity.Property(Function(e) e.FullName).HasMaxLength(300)
                entity.Property(Function(e) e.Sector).HasMaxLength(200)
                entity.Property(Function(e) e.Description).HasMaxLength(1000)
                entity.Property(Function(e) e.ContactEmail).HasMaxLength(200)
                entity.Property(Function(e) e.ContactPhone).HasMaxLength(20)
                entity.Property(Function(e) e.Website).HasMaxLength(200)
                entity.HasIndex(Function(e) e.SetaCode).IsUnique()
                entity.HasIndex(Function(e) e.IsActive)
            End Sub)
            
            ' Programme configuration
            modelBuilder.Entity(Of Programme)(Sub(entity)
                entity.HasKey(Function(e) e.Id)
                entity.Property(Function(e) e.QualificationCode).HasMaxLength(50).IsRequired()
                entity.Property(Function(e) e.Title).HasMaxLength(300).IsRequired()
                entity.Property(Function(e) e.Description).HasMaxLength(2000)
                entity.Property(Function(e) e.Sector).HasMaxLength(200)
                entity.Property(Function(e) e.ProgrammeType).HasMaxLength(100)
                entity.HasOne(Function(e) e.Seta) _
                      .WithMany() _
                      .HasForeignKey(Function(e) e.SetaId) _
                      .OnDelete(DeleteBehavior.Restrict)
                entity.HasIndex(Function(e) e.QualificationCode)
                entity.HasIndex(Function(e) e.SetaId)
                entity.HasIndex(Function(e) e.IsActive)
            End Sub)
            
            ' Contract configuration
            modelBuilder.Entity(Of Contract)(Sub(entity)
                entity.HasKey(Function(e) e.Id)
                entity.Property(Function(e) e.ContractNumber).HasMaxLength(100).IsRequired()
                entity.Property(Function(e) e.ProviderId).HasMaxLength(100)
                entity.Property(Function(e) e.ProviderName).HasMaxLength(300)
                entity.Property(Function(e) e.FundingAmount).HasPrecision(18, 2)
                entity.Property(Function(e) e.Notes).HasMaxLength(2000)
                entity.Property(Function(e) e.CreatedBy).HasMaxLength(100)
                entity.Property(Function(e) e.UpdatedBy).HasMaxLength(100)
                entity.HasOne(Function(e) e.Learner) _
                      .WithMany(Function(e) e.Contracts) _
                      .HasForeignKey(Function(e) e.LearnerId) _
                      .OnDelete(DeleteBehavior.Restrict)
                entity.HasOne(Function(e) e.Seta) _
                      .WithMany(Function(e) e.Contracts) _
                      .HasForeignKey(Function(e) e.SetaId) _
                      .OnDelete(DeleteBehavior.Restrict)
                entity.HasOne(Function(e) e.Programme) _
                      .WithMany(Function(e) e.Contracts) _
                      .HasForeignKey(Function(e) e.ProgrammeId) _
                      .OnDelete(DeleteBehavior.Restrict)
                entity.HasIndex(Function(e) e.ContractNumber).IsUnique()
                entity.HasIndex(Function(e) e.LearnerId)
                entity.HasIndex(Function(e) e.SetaId)
                entity.HasIndex(Function(e) e.Status)
                entity.HasIndex(Function(e) e.IsActive)
            End Sub)
        End Sub
    End Class
End Namespace
