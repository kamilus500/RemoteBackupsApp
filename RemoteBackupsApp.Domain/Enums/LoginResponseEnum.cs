namespace RemoteBackupsApp.Domain.Enums
{
    public enum LoginResponseEnum
    {
        Error = -1,
        NotCorrectPassword = 0,
        Success = 1,
        Logged = 2,
        Banned = 3
    }
}
