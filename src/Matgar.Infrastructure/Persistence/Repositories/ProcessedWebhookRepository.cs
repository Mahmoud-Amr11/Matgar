using Matgar.Application.Abstractions.Persistence.Repositories;
using Matgar.Domain.Entities;
using Matgar.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Matgar.Infrastructure.Persistence.Repositories
{
    internal class ProcessedWebhookRepository : IProcessedWebhookRepository
    {
        private readonly ApplicationDbContext _context;

        public ProcessedWebhookRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> TryRegisterAsync(string eventKey, CancellationToken cancellationToken = default)
        {
            try
            {
                var exists = await _context.Set<ProcessedWebhook>()
                    .AnyAsync(w => w.EventKey == eventKey, cancellationToken);

                if (exists) return false;

                _context.Set<ProcessedWebhook>().Add(new ProcessedWebhook { EventKey = eventKey });
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
