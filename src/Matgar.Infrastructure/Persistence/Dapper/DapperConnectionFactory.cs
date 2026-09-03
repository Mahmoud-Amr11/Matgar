using Matgar.Application.Abstractions.Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Matgar.Infrastructure.Persistence.Dapper
{
    internal class DapperConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public DapperConnectionFactory(IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "DefaultConnection is not configured.");
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
