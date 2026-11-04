using MediatR;
using Shopii.Application.Common.Models;
using Shopii.Application.Features.Users.DTOs;
using Shopii.Application.Interfaces.Persistence;

namespace Shopii.Application.Features.Users.Queries.GetAllUsers
{
    public sealed class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, PagedResult<UserDTO>>
    {
        private readonly IUserRepository _userRepository;

        public GetAllUsersQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<PagedResult<UserDTO>> Handle(
            GetAllUsersQuery request,
            CancellationToken cancellationToken)
        {
            // Fetch only the requested page from the database.
            var (users, totalCount) =
                await _userRepository.GetPagedAsync(
                    request.PageNumber,
                    request.PageSize,
                    cancellationToken);

            // Convert domain entities into DTOs.
            var userDTOs = users
                .Select(user => user.ToDTO())
                .ToList();

            // Return the users together with pagination metadata.
            return new PagedResult<UserDTO>(
                Items: userDTOs,
                PageNumber: request.PageNumber,
                PageSize: request.PageSize,
                TotalCount: totalCount);
        }
    }
}