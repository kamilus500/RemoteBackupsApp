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
            services.Configure<SmtpSettings>(settings => configuration.GetSection("SmtpSettings").Bind(settings));

            services.AddSingleton<DatabaseContext>();
            services.AddTransient<IBackupService, BackupService>();
            services.AddTransient<IFileService, FileService>();
            services.AddTransient<IEncryptionService, EncryptionService>();
            services.AddTransient<IAuthenticationService, AuthenticationService>();
            services.AddTransient<IUserContext, UserContext>();
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<IEmailService, EmailService>();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(120);
            });
        }

        public static void AddFormOptions(this IServiceCollection services)
        {
            services.Configure<FormOptions>(o =>
            {
                o.ValueLengthLimit = int.MaxValue;
                o.MultipartBodyLengthLimit = 4L * 1024L * 1024L * 1024L;
                o.MultipartBoundaryLengthLimit = int.MaxValue;
                o.MultipartHeadersCountLimit = int.MaxValue;
                o.MultipartHeadersLengthLimit = int.MaxValue;
                o.BufferBodyLengthLimit = 4L * 1024L * 1024L * 1024L;
                o.BufferBody = true;
                o.ValueCountLimit = int.MaxValue;
            });

            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = Int64.MaxValue;
            });
        }
    }
}
