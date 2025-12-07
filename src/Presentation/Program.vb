Imports Microsoft.AspNetCore.Builder
Imports Microsoft.Extensions.DependencyInjection
Imports Microsoft.Extensions.Hosting
Imports Microsoft.Extensions.Configuration
Imports Hangfire
Imports IDP.Infrastructure.DependencyInjection
Imports IDP.Infrastructure.BackgroundJobs

Module Program
    Sub Main(args As String())
        Dim builder = WebApplication.CreateBuilder(args)
        
        ' Add services to the container
        builder.Services.AddControllers()
        builder.Services.AddEndpointsApiExplorer()
        builder.Services.AddSwaggerGen()
        
        ' CORS
        builder.Services.AddCors(Sub(options)
            options.AddPolicy("AllowReactApp", Sub(builder)
                builder.WithOrigins("http://localhost:5173", "http://localhost:3000") _
                       .AllowAnyHeader() _
                       .AllowAnyMethod() _
                       .AllowCredentials()
            End Sub)
        End Sub)
        
        ' Get connection strings
        Dim connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        Dim checkIdBaseUrl = builder.Configuration("ExternalApis:CheckIdBaseUrl")
        
        ' Add Infrastructure layer
        builder.Services.AddInfrastructure(connectionString, checkIdBaseUrl)
        
        Dim app = builder.Build()
        
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
        
        app.Run()
    End Sub
End Module
