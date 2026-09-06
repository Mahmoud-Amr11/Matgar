using Matgar.Application.Abstractions.Repositories;
using Matgar.Domain.Entities;
using Matgar.Infrastructure.Persistence.Contexts;

namespace Matgar.Infrastructure.Persistence.Repositories
{
    internal class ProductVariantRepository : GenericRepository<ProductVariant>, IProductVariantRepository
    {
        public ProductVariantRepository(ApplicationDbContext context) : base(context) { }
    }
}
