using MediatR;
using Shopii.Application.Features.Users.DTOs;
using Shopii.Application.Interfaces.Persistence;
using Shopii.Application.Interfaces.Services;

namespace Shopii.Application.Features.Users.Queries.GetOwnProfile
{
    public class GetOwnProfileQueryHandler
        : IRequestHandler<GetOwnProfileQuery, UserDTO?>
    {
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetOwnProfileQueryHandler(
            IUserRepository userRepository,
            ICurrentUserService currentUserService)
        {
            _userRepository = userRepository;
            _currentUserService = currentUserService;
        }

        public GetOwnProfileQueryHandler(
            ICurrentUserService currentUserService,
            IUserRepository userRepository)
            : this(userRepository, currentUserService)
        {
        }

        public async Task<UserDTO?> Handle(
            GetOwnProfileQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId
                ?? throw new UnauthorizedException(
                    "A logged-in user is required.");

            var user = await _userRepository.GetByIdAsync(
                userId, cancellationToken);

            return user?.ToDTO();
        }
    }
}