Imports Microsoft.Extensions.DependencyInjection
Imports Microsoft.EntityFrameworkCore
Imports Refit
Imports Hangfire
Imports Hangfire.SqlServer
Imports IDP.Application.Interfaces
Imports IDP.Application.Services
Imports IDP.Infrastructure.Persistence
Imports IDP.Infrastructure.Persistence.Repositories
Imports IDP.Infrastructure.External
Imports IDP.Infrastructure.BackgroundJobs

Namespace DependencyInjection
    Public Module DependencyInjection
        Public Function AddInfrastructure(services As IServiceCollection, connectionString As String, checkIdBaseUrl As String) As IServiceCollection
            ' Database
            services.AddDbContext(Of ApplicationDbContext)(
                Sub(options)
                    options.UseSqlServer(connectionString,
                        Sub(sqlOptions)
                            sqlOptions.MigrationsAssembly("IDP.Migrations")
                        End Sub)
                End Sub
            )
            
            ' Repositories
            services.AddScoped(Of ILearnerRepository, LearnerRepository)()
            services.AddScoped(Of IDuplicationFlagRepository, DuplicationFlagRepository)()
            services.AddScoped(Of IDuplicationRuleRepository, DuplicationRuleRepository)()
            
            ' Application Services
            services.AddScoped(Of LearnerService)()
            services.AddScoped(Of IDuplicationDetectionService, DuplicationDetectionService)()
            
            ' External APIs
            services.AddRefitClient(Of ICheckIdApi)() _
                .ConfigureHttpClient(Sub(c) c.BaseAddress = New Uri(checkIdBaseUrl))
            
            services.AddScoped(Of IExternalVerificationService, ExternalVerificationService)()
            
            ' Background Jobs
            services.AddScoped(Of DuplicationCheckJob)()
            
            ' Hangfire
            services.AddHangfire(Sub(config)
                config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180) _
                      .UseSimpleAssemblyNameTypeSerializer() _
                      .UseRecommendedSerializerSettings() _
                      .UseSqlServerStorage(connectionString, New SqlServerStorageOptions() With {
                          .CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                          .SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                          .QueuePollInterval = TimeSpan.Zero,
                          .UseRecommendedIsolationLevel = True,
                          .DisableGlobalLocks = True
                      })
            End Sub)
            
            services.AddHangfireServer()
            
            Return services
        End Function
    End Module
End Namespace
