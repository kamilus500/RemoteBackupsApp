using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using RemoteBackupsApp.Domain.Interfaces;
using RemoteBackupsApp.Infrastructure.Hubs;
using RemoteBackupsApp.Infrastructure.Repositories;
using RemoteBackupsApp.Infrastructure.Services;

namespace RemoteBackupsApp.Infrastructure
{
    public static class Extensions
    {
        public static void AddInfrastructure(this IServiceCollection services)
        {
            services.AddSignalR();

            services.AddScoped<ISqlService, SqlService>();
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IFilesRepository, FilesRepository>();
            services.AddScoped<IFileUploadProcessRepository, FileUploadProcessRepository>();

            services.AddSingleton<IFileQueueService, FileQueueService>();
            services.AddHostedService<FileBackgroundService>();

            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 100 * 1024 * 1024;
            });
        }

        public static void AddApplication(this WebApplication app)
        {
            app.MapHub<UploadHub>("/uploadHub");
        }
    }
}
