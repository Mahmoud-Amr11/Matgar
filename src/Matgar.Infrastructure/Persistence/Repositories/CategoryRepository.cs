using Matgar.Application.Abstractions.Repositories;
using Matgar.Domain.Entities;
using Matgar.Infrastructure.Persistence.Contexts;

namespace Matgar.Infrastructure.Persistence.Repositories
{
    internal class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
