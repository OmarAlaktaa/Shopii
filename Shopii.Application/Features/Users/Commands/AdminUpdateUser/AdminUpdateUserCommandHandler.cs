using MediatR;
using Shopii.Application.Features.Users.DTOs;
using Shopii.Application.Interfaces.Authentication;
using Shopii.Application.Interfaces.Persistence;
using Shopii.Application.Interfaces.Services;

namespace Shopii.Application.Features.Users.Commands.AdminUpdateUser
{
    public class AdminUpdateUserCommandHandler
        : IRequestHandler<AdminUpdateUserCommand, UserDTO>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ICurrentUserService _currentUserService;

        public AdminUpdateUserCommandHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            ICurrentUserService currentUserService)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _currentUserService = currentUserService;
        }

        public AdminUpdateUserCommandHandler(
            IUserRepository userRepository,
            ICurrentUserService currentUserService,
            IPasswordHasher passwordHasher)
            : this(userRepository, passwordHasher, currentUserService)
        {
        }

        public AdminUpdateUserCommandHandler(
            ICurrentUserService currentUserService,
            IUserRepository userRepository,
            IPasswordHasher passwordHasher)
            : this(userRepository, passwordHasher, currentUserService)
        {
        }

        public AdminUpdateUserCommandHandler(
            ICurrentUserService currentUserService,
            IPasswordHasher passwordHasher,
            IUserRepository userRepository)
            : this(userRepository, passwordHasher, currentUserService)
        {
        }

        public AdminUpdateUserCommandHandler(
            IPasswordHasher passwordHasher,
            IUserRepository userRepository,
            ICurrentUserService currentUserService)
            : this(userRepository, passwordHasher, currentUserService)
        {
        }

        public AdminUpdateUserCommandHandler(
            IPasswordHasher passwordHasher,
            ICurrentUserService currentUserService,
            IUserRepository userRepository)
            : this(userRepository, passwordHasher, currentUserService)
        {
        }

        public async Task<UserDTO> Handle(
            AdminUpdateUserCommand request,
            CancellationToken cancellationToken)
        {
            // Defense in depth: the controller already requires the "Admin"
            // policy, but the rule is enforced here too.
            var currentUserId = _currentUserService.UserId
                ?? throw new UnauthorizedException("A logged-in user is required.");

            if (!_currentUserService.IsAdmin)
            {
                throw new UnauthorizedException(
                    "Only admins can update other users.");
            }

            var user = await _userRepository.GetByIdAsync(
                request.UserId,
                cancellationToken);

            if (user is null)
            {
                throw new NotFoundException(
                    $"User with id '{request.UserId}' was not found.");
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
            user.RoleId = request.RoleId;
            user.IsVerified = request.IsVerified;

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