using System.Data;

namespace Matgar.Application.Abstractions.Persistence.Dapper
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
