using MediatR;
using Shopii.Application.Features.Users.DTOs;
using Shopii.Application.Interfaces.Authentication;
using Shopii.Application.Interfaces.Persistence;
using Shopii.Domain.Entities.Users;

namespace Shopii.Application.Features.Users.Commands.RegisterUser
{
    public class RegisterUserCommandHandler
        : IRequestHandler<RegisterUserCommand, UserDTO>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IPasswordHasher _passwordHasher;

        public RegisterUserCommandHandler(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _passwordHasher = passwordHasher;
        }

        public RegisterUserCommandHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IRoleRepository roleRepository)
            : this(userRepository, roleRepository, passwordHasher)
        {
        }

        public RegisterUserCommandHandler(
            IRoleRepository roleRepository,
            IUserRepository userRepository,
            IPasswordHasher passwordHasher)
            : this(userRepository, roleRepository, passwordHasher)
        {
        }

        public RegisterUserCommandHandler(
            IRoleRepository roleRepository,
            IPasswordHasher passwordHasher,
            IUserRepository userRepository)
            : this(userRepository, roleRepository, passwordHasher)
        {
        }

        public RegisterUserCommandHandler(
            IPasswordHasher passwordHasher,
            IUserRepository userRepository,
            IRoleRepository roleRepository)
            : this(userRepository, roleRepository, passwordHasher)
        {
        }

        public RegisterUserCommandHandler(
            IPasswordHasher passwordHasher,
            IRoleRepository roleRepository,
            IUserRepository userRepository)
            : this(userRepository, roleRepository, passwordHasher)
        {
        }

        public async Task<UserDTO> Handle(
            RegisterUserCommand request,
            CancellationToken cancellationToken)
        {
            // Only an active account owns an email address. Soft-deleted users
            // (DeletedAt != null) keep their row and real email for history, but
            // do not block a brand-new profile from registering the same address.
            // The repository predicate is explicit and ignores query filters.
            var emailIsTaken = await _userRepository.AnyActiveByEmailAsync(
                request.Email,
                cancellationToken);

            if (emailIsTaken)
            {
                throw new ValidationException(
                    "User with this email already exists.");
            }

            // Resolve the Customer role server-side. Public registration must
            // never let the client choose a role, so the role is looked up from
            // the existing role data instead of trusting request input.
            var customerRole = await _roleRepository.GetByTitleAsync(
                "Customer",
                cancellationToken);

            if (customerRole is null)
            {
                throw new NotFoundException(
                    "Role 'Customer' was not found.");
            }

            string hashedPassword = _passwordHasher.HashPassword(
                request.Password,
                out string salt);

            var user = new User
            {
                UserId = Guid.NewGuid().ToString(),
                Email = request.Email,
                Password = hashedPassword,
                Salt = salt,
                RoleId = customerRole.RoleId,
                Role = customerRole,
                IsVerified = false,
                CreatedAt = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user, cancellationToken);

            return user.ToDTO();
        }
    }
}