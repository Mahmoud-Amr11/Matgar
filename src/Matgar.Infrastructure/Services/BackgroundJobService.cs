using Hangfire;
using Matgar.Application.Abstractions.Services;
using System.Linq.Expressions;

namespace Matgar.Infrastructure.Services
{
    internal class BackgroundJobService(IBackgroundJobClient _backgroundJobClient) : IBackgroundJobService
    {
        public string Enqueue<TJob>(Expression<Func<TJob, Task>> job) =>
            _backgroundJobClient.Enqueue(job);

        public string Schedule<TJob>(Expression<Func<TJob, Task>> job, TimeSpan delay)
          => _backgroundJobClient.Schedule(job, delay);
    }
}
