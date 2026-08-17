namespace Matgar.Infrastructure.Persistence.Seeders
{
    internal class DataSeederRunner
    {
        private readonly IEnumerable<IDataSeeder> _dataSeeder;

        public DataSeederRunner(IEnumerable<IDataSeeder> dataSeeder)
        {
            _dataSeeder = dataSeeder;
        }



        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            foreach (var dataSeeder in _dataSeeder)
            {
                await dataSeeder.SeedAsync(cancellationToken);
            }
        }
    }
}
