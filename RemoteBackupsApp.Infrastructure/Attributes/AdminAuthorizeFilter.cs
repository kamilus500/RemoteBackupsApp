using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using RemoteBackupsApp.Infrastructure.Services.Interfaces;

namespace RemoteBackupsApp.Infrastructure.Attributes
{
    public class AdminAuthorizeFilter : IAuthorizationFilter
    {
        private readonly IUserContext _userContext;
        public AdminAuthorizeFilter(IUserContext userContext)
        {
            _userContext = userContext;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = _userContext.GetUser().Result;

            if (!user.RoleName.Equals("Admin"))
            {
                context.Result = new ViewResult { ViewName = "Error" };
            }
        }
    }
}
