namespace Shopii.Application.Common.Models
{
    /// <summary>
    /// Implemented by every query that returns a page of results.
    ///
    /// Kept as an interface (rather than a shared record base) so each feature
    /// can keep its strongly typed response while all paginated queries share
    /// the same validation rules.
    /// </summary>
    public interface IPagedRequest
    {
        int PageNumber { get; }

        int PageSize { get; }
    }
}
