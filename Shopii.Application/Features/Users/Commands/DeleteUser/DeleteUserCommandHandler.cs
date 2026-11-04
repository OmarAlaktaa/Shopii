using MediatR;
using Shopii.Application.Interfaces.Persistence;
using Shopii.Application.Interfaces.Services;

namespace Shopii.Application.Features.Users.Commands.DeleteUser
{
    public class DeleteUserCommandHandler
        : IRequestHandler<DeleteUserCommand, bool>
    {
        private readonly IUserRepository _userRepository;
        private readonly ICurrentUserService _currentUserService;

        public DeleteUserCommandHandler(
            IUserRepository userRepository,
            ICurrentUserService currentUserService)
        {
            _userRepository = userRepository;
            _currentUserService = currentUserService;
        }

        public DeleteUserCommandHandler(
            ICurrentUserService currentUserService,
            IUserRepository userRepository)
            : this(userRepository, currentUserService)
        {
        }

        public async Task<bool> Handle(
            DeleteUserCommand request,
            CancellationToken cancellationToken)
        {
            // The authenticated identity is the single source of truth for
            // who is performing the deletion. Never trust a caller-supplied id.
            var currentUserId = _currentUserService.UserId
                ?? throw new UnauthorizedException(
                    "A logged-in user is required.");

            var targetUserId = request.UserId;

            var isSelf = string.Equals(
                currentUserId, targetUserId, StringComparison.Ordinal);

            if (!isSelf && !_currentUserService.IsAdmin)
            {
                throw new ForbiddenException(
                    "You are not allowed to delete this user.");
            }

            var deleted = await _userRepository.DeleteAsync(
                targetUserId,
                cancellationToken);

            if (!deleted)
            {
                throw new NotFoundException(
                    $"User with id '{targetUserId}' was not found.");
            }

            return true;
        }
    }
}