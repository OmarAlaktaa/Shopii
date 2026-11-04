using MediatR;
using Shopii.Application.Common.Models;
using Shopii.Application.Features.Users.DTOs;

namespace Shopii.Application.Features.Users.Queries.GetAllUsers
{
    // Describes the request to retrieve a specific page of users.
    public sealed record GetAllUsersQuery(
        int PageNumber = 1,
        int PageSize = 10
    ) : IRequest<PagedResult<UserDTO>>, IPagedRequest;
}