using System.Linq.Expressions;

namespace Matgar.Application.Abstractions.Services
{
    public interface IBackgroundJobService
    {
        string Enqueue<TJob>(Expression<Func<TJob, Task>> job);

        string Schedule<TJob>(
            Expression<Func<TJob, Task>> job,
            TimeSpan delay);
    }
}
