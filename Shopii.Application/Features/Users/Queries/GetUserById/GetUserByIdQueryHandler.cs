using MediatR;
using Shopii.Application.Features.Users.DTOs;
using Shopii.Application.Interfaces.Persistence;
using Shopii.Application.Interfaces.Services;

namespace Shopii.Application.Features.Users.Queries.GetUserById
{
    public class GetUserByIdQueryHandler
        : IRequestHandler<GetUserByIdQuery, UserDTO?>
    {
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserService _currentUserService;

        public GetUserByIdQueryHandler(IUserRepository userRepository)
            : this(userRepository, new NullCurrentUserService())
        {
        }

        public GetUserByIdQueryHandler(
            IUserRepository userRepository,
            ICurrentUserService currentUserService)
        {
            _userRepository = userRepository;
            _currentUserService = currentUserService;
        }

        public GetUserByIdQueryHandler(
            ICurrentUserService currentUserService,
            IUserRepository userRepository)
            : this(userRepository, currentUserService)
        {
        }

        public async Task<UserDTO?> Handle(
            GetUserByIdQuery request,
            CancellationToken cancellationToken)
        {
            // The caller's identity and role come only from the validated JWT.
            var currentUserId = _currentUserService.UserId
                ?? throw new UnauthorizedException(
                    "A logged-in user is required.");

            var isAdmin = _currentUserService.IsAdmin;

            // Non-admins may only read their own record. Returning null (rather
            // than a distinct forbidden result) keeps the response identical to
            // a missing record so we do not leak whether the target exists.
            if (!isAdmin && !string.Equals(
                    currentUserId, request.UserId, StringComparison.Ordinal))
            {
                return null;
            }

            var user = await _userRepository.GetByIdAsync(
                request.UserId, cancellationToken);

            return user?.ToDTO();
        }

        private sealed class NullCurrentUserService : ICurrentUserService
        {
            public string? UserId => null;
            public string? Role => null;
            public bool IsAdmin => false;
        }
    }
}