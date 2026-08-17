namespace Matgar.Infrastructure.Persistence.Seeders
{
    internal interface IDataSeeder
    {
        public int Order { get; }
        Task SeedAsync(CancellationToken cancellationToken);

    }
}
