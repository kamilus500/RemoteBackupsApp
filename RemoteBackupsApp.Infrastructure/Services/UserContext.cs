using Dapper;
using Microsoft.AspNetCore.Http;
using RemoteBackupsApp.Domain.ViewModels.User;
using RemoteBackupsApp.Infrastructure.Initializers;
using RemoteBackupsApp.Infrastructure.Services.Interfaces;
using System.Data;

namespace RemoteBackupsApp.Infrastructure.Services
{
    public class UserContext : IUserContext
    {
        private readonly IDbConnection _dbContext;
        private readonly ISession _session;
        public UserContext(DatabaseContext databaseContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = databaseContext.CreateConnection();
            _session = httpContextAccessor.HttpContext.Session;
        }

        public async Task<UserViewModel> GetUser()
        {
            var actuallyLogUserName = _session.GetString("userName");

            var parameter = new { UserName = actuallyLogUserName };

            var user = await _dbContext.QueryFirstOrDefaultAsync<UserViewModel>("SELECT CAST(Id AS NVARCHAR(36)) AS Id, UserName, Email FROM UserTable WHERE UserName = @UserName", parameter);

            if (user is null)
                throw new ArgumentNullException(nameof(user));

            return user;
        }

        public async Task<bool> IsUserLogIn()
        {
            var actuallyLogUserName = _session.GetString("userName");

            if (actuallyLogUserName is null)
                return false;

            var parameter = new { UserName = actuallyLogUserName };

            return await _dbContext.QueryFirstOrDefaultAsync<bool>("SELECT IsLogin FROM UserTable WHERE UserName = @UserName", parameter);
        }
    }
}
