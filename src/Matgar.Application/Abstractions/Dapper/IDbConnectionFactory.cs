using System.Data;

namespace Matgar.Application.Abstractions.Dapper
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
