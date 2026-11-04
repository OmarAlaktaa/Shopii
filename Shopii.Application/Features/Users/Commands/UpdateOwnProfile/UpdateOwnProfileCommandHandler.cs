using MediatR;
using Shopii.Application.Features.Users.DTOs;
using Shopii.Application.Interfaces.Authentication;
using Shopii.Application.Interfaces.Persistence;
using Shopii.Application.Interfaces.Services;

namespace Shopii.Application.Features.Users.Commands.UpdateOwnProfile
{
    public class UpdateOwnProfileCommandHandler
        : IRequestHandler<UpdateOwnProfileCommand, UserDTO>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ICurrentUserService _currentUserService;

        public UpdateOwnProfileCommandHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            ICurrentUserService currentUserService)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _currentUserService = currentUserService;
        }

        public UpdateOwnProfileCommandHandler(
            IUserRepository userRepository,
            ICurrentUserService currentUserService,
            IPasswordHasher passwordHasher)
            : this(userRepository, passwordHasher, currentUserService)
        {
        }

        public UpdateOwnProfileCommandHandler(
            ICurrentUserService currentUserService,
            IUserRepository userRepository,
            IPasswordHasher passwordHasher)
            : this(userRepository, passwordHasher, currentUserService)
        {
        }

        public UpdateOwnProfileCommandHandler(
            ICurrentUserService currentUserService,
            IPasswordHasher passwordHasher,
            IUserRepository userRepository)
            : this(userRepository, passwordHasher, currentUserService)
        {
        }

        public UpdateOwnProfileCommandHandler(
            IPasswordHasher passwordHasher,
            IUserRepository userRepository,
            ICurrentUserService currentUserService)
            : this(userRepository, passwordHasher, currentUserService)
        {
        }

        public UpdateOwnProfileCommandHandler(
            IPasswordHasher passwordHasher,
            ICurrentUserService currentUserService,
            IUserRepository userRepository)
            : this(userRepository, passwordHasher, currentUserService)
        {
        }

        public async Task<UserDTO> Handle(
            UpdateOwnProfileCommand request,
            CancellationToken cancellationToken)
        {
            // The authenticated identity is the single source of truth for
            // which account is being updated. Never trust a client-supplied id.
            var userId = _currentUserService.UserId
                ?? throw new UnauthorizedException("A logged-in user is required.");

            var user = await _userRepository.GetByIdAsync(
                userId,
                cancellationToken);

            if (user is null)
            {
                throw new NotFoundException(
                    $"User with id '{userId}' was not found.");
            }

            // Only reject the new email when a live user already owns it.
            // Soft-deleted users released their address on delete, so they are
            // intentionally not considered here.
            if (!string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
            {
                var existing = await _userRepository.GetByEmailAsync(
                    request.Email,
                    cancellationToken);

                if (existing is not null && existing.UserId != user.UserId)
                {
                    throw new ValidationException(
                        "User with this email already exists.");
                }
            }

            user.Email = request.Email;

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                user.Password = _passwordHasher.HashPassword(
                    request.Password,
                    out string salt);
                user.Salt = salt;
            }

            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user, cancellationToken);

            return user.ToDTO();
        }
    }
}