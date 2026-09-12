using Dapper;
using Matgar.Application.Abstractions.Dapper;
using Matgar.Application.Abstractions.Queries.Addresses;
using Matgar.Application.Features.Addresses.Queries.Responses;

namespace Matgar.Infrastructure.Persistence.Queries.Addresses
{
    internal sealed class AddressQueries : IAddressQueries
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public AddressQueries(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<List<AddressResponse>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT
                    a.Id AS AddressId,
                    a.FullAddress,
                    a.City,
                    a.Governorate,
                    a.PhoneNumber,
                    a.IsDefault
                FROM Addresses a
                WHERE a.UserId = @UserId
                  AND (a.IsDeleted = 0 OR a.IsDeleted IS NULL)
                ORDER BY a.IsDefault DESC, a.CreatedAt DESC;
                """;

            var addresses = (await connection.QueryAsync<AddressResponse>(
                new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken))).ToList();

            return addresses;
        }
    }
}