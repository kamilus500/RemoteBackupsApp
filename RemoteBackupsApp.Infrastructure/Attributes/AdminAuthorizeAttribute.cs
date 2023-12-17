using Microsoft.AspNetCore.Mvc;

namespace RemoteBackupsApp.Infrastructure.Attributes
{
    public class AdminAuthorizeAttribute : TypeFilterAttribute
    {
        public AdminAuthorizeAttribute() : base(typeof(AdminAuthorizeFilter))
        {
        }
    }
}
