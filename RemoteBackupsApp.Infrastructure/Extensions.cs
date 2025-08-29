using Microsoft.Extensions.DependencyInjection;
using RemoteBackupsApp.Domain.Interfaces;
using RemoteBackupsApp.Infrastructure.Repositories;
using RemoteBackupsApp.Infrastructure.Services;

namespace RemoteBackupsApp.Infrastructure
{
    public static class Extensions
    {
        public static void AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<ISqlService, SqlService>();
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IFilesRepository, FilesRepository>();

            services.AddSingleton<IFileQueueService, FileQueueService>();
            services.AddHostedService<FileBackgroundService>();
        }
    }
}
