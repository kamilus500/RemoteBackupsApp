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

        public async Task BanUser(string userName)
        {
            var parameter = new
            {
                UserName = userName
            };

            await _dbContext.ExecuteAsync("BanUser", parameter, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<UserViewModel>> GetAllUsers()
            => await _dbContext.QueryAsync<UserViewModel>("SELECT CAST(U.Id AS NVARCHAR(36)) AS Id, U.UserName, U.Email, R.Name AS RoleName, U.IsBan AS IsBanned FROM UserTable U INNER JOIN RoleTable R ON R.Id= U.RoleId WHERE RoleId = 1");

        public async Task<UserViewModel> GetUser()
        {
            var actuallyLogUserName = _session.GetString("userName");

            var parameter = new { UserName = actuallyLogUserName };

            var user = await _dbContext.QueryFirstOrDefaultAsync<UserViewModel>("SELECT CAST(U.Id AS NVARCHAR(36)) AS Id, U.UserName, U.Email, R.Name AS RoleName FROM UserTable U INNER JOIN RoleTable R ON R.Id= U.RoleId WHERE UserName = @UserName", parameter);

            if (user is null)
                throw new ArgumentNullException(nameof(user));

            return user;
        }

        public async Task<bool> IsInRole(string roleName)
        {
            var actuallyLogUserName = _session.GetString("userName");

            if (actuallyLogUserName is null)
                return false;

            var parameter = new { UserName = actuallyLogUserName };

            var role = await _dbContext.QueryFirstOrDefaultAsync<string>("SELECT R.Name AS RoleName FROM UserTable U INNER JOIN RoleTable R ON R.Id= U.RoleId WHERE UserName = @UserName", parameter);

            return role!.Equals(roleName);
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
