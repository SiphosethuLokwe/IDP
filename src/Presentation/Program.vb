Imports Microsoft.AspNetCore.Builder
Imports Microsoft.Extensions.DependencyInjection
Imports Microsoft.Extensions.Hosting
Imports Microsoft.Extensions.Configuration
Imports Hangfire
Imports IDP.Infrastructure.DependencyInjection
Imports IDP.Infrastructure.BackgroundJobs
Imports IDP.Infrastructure.Persistence

Module Program
    Async Function MainAsync(args As String()) As Task
        Dim builder = WebApplication.CreateBuilder(args)
        
        ' Add services to the container
        builder.Services.AddControllers()
        builder.Services.AddEndpointsApiExplorer()
        builder.Services.AddSwaggerGen()
        
        ' CORS
        builder.Services.AddCors(Sub(options)
            options.AddPolicy("AllowReactApp", Sub(policyBuilder)
                policyBuilder.WithOrigins("http://localhost:5173", "http://localhost:3000") _
                       .AllowAnyHeader() _
                       .AllowAnyMethod() _
                       .AllowCredentials()
            End Sub)
        End Sub)
        
        ' Get connection strings
        Dim connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        Dim checkIdBaseUrl = builder.Configuration("ExternalApis:CheckIdBaseUrl")
        
        ' Add Infrastructure layer
        AddInfrastructure(builder.Services, connectionString, checkIdBaseUrl)
        
        Dim app = builder.Build()
        
        ' Seed database with initial data
        Using scope = app.Services.CreateScope()
            Dim dbContext = scope.ServiceProvider.GetRequiredService(Of ApplicationDbContext)()
            Dim seeder = New DatabaseSeeder(dbContext)
            Await seeder.SeedAsync()
        End Using
        
        ' Configure the HTTP request pipeline
        If app.Environment.IsDevelopment() Then
            app.UseSwagger()
            app.UseSwaggerUI()
        End If
        
        app.UseHttpsRedirection()
        app.UseCors("AllowReactApp")
        app.UseAuthorization()
        
        ' Hangfire Dashboard
        app.UseHangfireDashboard("/hangfire")
        
        ' Schedule recurring jobs
        DuplicationCheckJob.ScheduleRecurringJobs()
        
        app.MapControllers()
        
        Await app.RunAsync()
    End Function
    
    Sub Main(args As String())
        MainAsync(args).GetAwaiter().GetResult()
    End Sub
End Module
