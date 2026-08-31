namespace Matgar.Application.Common.Pagination
{
    public sealed record PagedResult<T>(
      IReadOnlyList<T> Items,
      int Page,
      int PageSize,
      int TotalCount)
    {
        public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;
    }
}
