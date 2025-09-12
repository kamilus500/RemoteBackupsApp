using RemoteBackupsApp.Domain.Interfaces;

namespace RemoteBackupsApp.Infrastructure.Repositories;

public abstract class BaseRepository
{
    protected readonly ISqlService _sqlService;
    public BaseRepository(ISqlService sqlService)
    {
        _sqlService = sqlService ?? throw new ArgumentNullException(nameof(sqlService));
    }
}
