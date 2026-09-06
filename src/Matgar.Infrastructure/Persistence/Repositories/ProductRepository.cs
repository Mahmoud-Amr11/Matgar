using Matgar.Application.Abstractions.Repositories;
using Matgar.Domain.Entities;
using Matgar.Infrastructure.Persistence.Contexts;

namespace Matgar.Infrastructure.Persistence.Repositories
{
    internal class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context) { }
    }
}
