using Matgar.Application.Abstractions.Repositories;
using Matgar.Domain.Entities;
using Matgar.Infrastructure.Persistence.Contexts;

namespace Matgar.Infrastructure.Persistence.Repositories
{
    internal class ProductReviewRepository : GenericRepository<ProductReview>, IProductReviewRepository
    {
        public ProductReviewRepository(ApplicationDbContext context) : base(context) { }
    }
}
