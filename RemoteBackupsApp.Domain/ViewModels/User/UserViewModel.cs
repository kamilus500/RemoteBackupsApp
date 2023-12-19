namespace RemoteBackupsApp.Domain.ViewModels.User
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string RoleName { get; set; }
        public bool IsBanned { get; set; }
    }
}
