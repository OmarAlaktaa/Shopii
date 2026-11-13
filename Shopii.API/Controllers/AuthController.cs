using Microsoft.AspNetCore.Mvc;
using MediatR;
using Shopii.Application.Common.Auditing;
using Shopii.Application.Features.Users.Commands.RegisterUser;
using Shopii.Application.Features.Users.Commands.LoginUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Shopii.Application.Interfaces.Services;

namespace Shopii.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [EnableRateLimiting("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IAuditLogger _auditLogger;

        public AuthController(
            IMediator mediator,
            IAuditLogger auditLogger)
        {
            // mediator function is to send commands and
            // queries to their respective handlers.
            _mediator = mediator;
            _auditLogger = auditLogger;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [AllowAnonymous]
        public async Task<IActionResult> Register(
            [FromBody] RegisterUserCommand command,
            CancellationToken cancellationToken)
        {
            // Validation and "email already exists" failures are thrown as
            // ValidationException and translated to HTTP 400 by the exception
            // handling middleware.
            var result = await _mediator.Send(command, cancellationToken);

            await _auditLogger.LogAsync(
                new AuditEvent(
                    AuditAction.UserRegistered,
                    AuditEntityTypes.User,
                    result.UserId,
                    ActorUserId: result.UserId),
                cancellationToken);

            return Ok(result);
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [AllowAnonymous]
        public async Task<IActionResult> Login(
            [FromBody] LoginUserCommand command,
            CancellationToken cancellationToken)
        {
            // Invalid credentials are thrown as UnauthorizedException and
            // translated to HTTP 401 by the exception handling middleware.
            var token = await _mediator.Send(command, cancellationToken);
            return Ok(new { Token = token });
        }
    }
}
