namespace Shopii.Application.Common.Models
{
    public sealed record PagedResult<T>(
        IReadOnlyList<T> Items, int PageNumber, int PageSize, int TotalCount)
    {
        // Number of pages available for the requested page size.
        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

        public bool HasPreviousPage => PageNumber > 1;

        public bool HasNextPage => PageNumber < TotalPages;
    }
}