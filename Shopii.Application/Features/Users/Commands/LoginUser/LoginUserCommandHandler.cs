using MediatR;
using Microsoft.Extensions.Logging;
using Shopii.Application.Common.Auditing;
using Shopii.Application.Common.Logging;
using Shopii.Application.Interfaces.Authentication;
using Shopii.Application.Interfaces.Persistence;
using Shopii.Application.Interfaces.Services;

namespace Shopii.Application.Features.Users.Commands.LoginUser
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, string>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly IAuditLogger _auditLogger;
        private readonly ILogger<LoginUserCommandHandler> _logger;

        public LoginUserCommandHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            IAuditLogger auditLogger,
            ILogger<LoginUserCommandHandler> logger)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _auditLogger = auditLogger;
            _logger = logger;
        }

        public async Task<string> Handle(
            LoginUserCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(
                request.Email,
                cancellationToken);

            if (user == null || !_passwordHasher.VerifyPassword(
                request.Password,
                user.Salt,
                user.Password))
            {
                // Never log the attempted email or password.
                _logger.LogAuthenticationFailed();

                await _auditLogger.LogAsync(
                    new AuditEvent(
                        AuditAction.LoginFailed,
                        AuditEntityTypes.User,
                        EntityId: user?.UserId,
                        ActorUserId: user?.UserId,
                        Details: AuditDetails.Create(
                            ("Reason", "InvalidCredentials"))),
                    cancellationToken);

                throw new UnauthorizedException("Invalid email or password.");
            }

            // Enforce the existing verification policy: an account must be
            // verified before a token is issued.
            if (!user.IsVerified)
            {
                _logger.LogAccountNotVerified(user.UserId);

                await _auditLogger.LogAsync(
                    new AuditEvent(
                        AuditAction.LoginRejected,
                        AuditEntityTypes.User,
                        user.UserId,
                        ActorUserId: user.UserId,
                        Details: AuditDetails.Create(
                            ("Reason", "NotVerified"))),
                    cancellationToken);

                throw new UnauthorizedException(
                    "Account is not verified.");
            }

            // Update last login
            user.LastLogin = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user, cancellationToken);

            // Generate the token before writing the success audit. If token
            // generation fails, the audit trail must not claim a successful
            // login.
            var token = _tokenService.GenerateToken(user);

            await _auditLogger.LogAsync(
                new AuditEvent(
                    AuditAction.LoginSucceeded,
                    AuditEntityTypes.User,
                    user.UserId,
                    ActorUserId: user.UserId),
                cancellationToken);

            _logger.LogUserAuthenticated(user.UserId);

            return token;
        }
    }
}
