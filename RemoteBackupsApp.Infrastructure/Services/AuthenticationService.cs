using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using RemoteBackupsApp.Domain.ViewModels.Authentication;
using RemoteBackupsApp.Infrastructure.Initializers;
using RemoteBackupsApp.Infrastructure.Services.Interfaces;
using System.Data;

namespace RemoteBackupsApp.Infrastructure.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IDbConnection _dbContext;
        private readonly ISession _session;
        public AuthenticationService(DatabaseContext databaseContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = databaseContext.CreateConnection();
            _session = httpContextAccessor.HttpContext.Session; 
        }

        public async Task<int> Login(LoginViewModel loginViewModel)
        {
            try
            {
                var parameters = new DynamicParameters();

                parameters.Add("UserName", loginViewModel.UserName);
                parameters.Add("Password", loginViewModel.Password);
                parameters.Add("LoginResult", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

                await _dbContext.ExecuteAsync("LoginUser", parameters, commandType: CommandType.StoredProcedure);

                var isLoginSuccess = parameters.Get<int>("LoginResult");

                if (isLoginSuccess is 1)
                {
                    _session.SetString("userName", loginViewModel.UserName);
                }

                return isLoginSuccess;
            }
            catch
            {
                return -1;
            }
        }

        public async Task LogOut(string userName)
        {
            var parameter = new
            {
                UserName = userName
            };

            await _dbContext.ExecuteAsync("LogOut", parameter, commandType: CommandType.StoredProcedure);

            _session.Clear();
        }

        public async Task<bool> Register(RegisterViewModel registerViewModel)
        {
            try
            {
                var parameter = new
                {
                    Email = registerViewModel.Email,
                    UserName = registerViewModel.UserName,
                    Password = registerViewModel.Password
                };

                await _dbContext.ExecuteAsync("CreateNewUser", parameter, commandType: CommandType.StoredProcedure);

                return true;
            }
            catch (SqlException ex)
            {
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
