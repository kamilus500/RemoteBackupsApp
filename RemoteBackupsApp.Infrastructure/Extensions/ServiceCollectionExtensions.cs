using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RemoteBackupsApp.Domain.ViewModels.Configs;
using RemoteBackupsApp.Infrastructure.Initializers;
using RemoteBackupsApp.Infrastructure.Services;
using RemoteBackupsApp.Infrastructure.Services.Interfaces;

namespace RemoteBackupsApp.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var databaseContext = new DatabaseContext(configuration.GetConnectionString("conString"));

            services.Configure<SmtpSettings>(settings => configuration.GetSection("SmtpSettings").Bind(settings));

            services.AddSingleton(databaseContext);
            services.AddTransient<IBackupService, BackupService>();
            services.AddTransient<IFileService, FileService>();
            services.AddTransient<IEncryptionService, EncryptionService>();
            services.AddTransient<IAuthenticationService, AuthenticationService>();
            services.AddTransient<IUserContext, UserContext>();
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<IFileProcessingService, FileProcessingService>();

            services.AddSignalR();

            services.AddHangfire(config => config.UseSqlServerStorage(configuration.GetConnectionString("conString")+"Database = RemoteBackupDb"));

            services.AddHangfireServer();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(12);
            });
        }

        public static void AddFormOptions(this IServiceCollection services)
        {
            services.Configure<FormOptions>(o =>
            {
                o.MultipartBodyLengthLimit = 1024 * 1024 * 150;
            });
        }
    }
}
