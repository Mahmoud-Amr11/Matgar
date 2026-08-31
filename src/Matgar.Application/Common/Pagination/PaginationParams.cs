namespace Matgar.Application.Common.Pagination
{
    public sealed record PaginationParams
    {
        private const int MaxPageSize = 100;
        private const int DefaultPageSize = 20;


        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = DefaultPageSize;

        public int NormalizedPage => Page < 1 ? 1 : Page;
        public int NormalizedPageSize => PageSize is < 1 or > MaxPageSize ? DefaultPageSize : PageSize;

        public int Offset => (NormalizedPage - 1) * NormalizedPageSize;
    }
}
