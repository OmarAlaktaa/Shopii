using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopii.API.Models;
using Shopii.Application.Common.Auditing;
using Shopii.Application.Features.Users.Commands.AdminUpdateUser;
using Shopii.Application.Features.Users.Commands.DeleteUser;
using Shopii.Application.Features.Users.Commands.UpdateOwnProfile;
using Shopii.Application.Features.Users.Queries.GetAllUsers;
using Shopii.Application.Features.Users.Queries.GetOwnProfile;
using Shopii.Application.Features.Users.Queries.GetUserById;
using Shopii.Application.Interfaces.Services;

namespace Shopii.API.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        // MediatR dispatches commands and queries to their corresponding handlers.
        private readonly IMediator _mediator;
        private readonly IAuditLogger _auditLogger;

        public UsersController(
            IMediator mediator,
            IAuditLogger auditLogger)
        {
            _mediator = mediator;
            _auditLogger = auditLogger;
        }

        // GET: api/users?pageNumber=1&pageSize=10
        [Authorize(Policy = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var query = new GetAllUsersQuery(pageNumber, pageSize);
            var users = await _mediator.Send(query, cancellationToken);

            await _auditLogger.LogAsync(
                new AuditEvent(
                    AuditAction.UsersListViewed,
                    AuditEntityTypes.User,
                    Details: AuditDetails.Create(
                        ("PageNumber", users.PageNumber),
                        ("PageSize", users.PageSize),
                        ("TotalCount", users.TotalCount))),
                cancellationToken);

            return Ok(users);
        }

        // GET: api/users/me
        // Any authenticated user can retrieve their own profile. The account
        // is resolved from the validated JWT, never from request parameters.
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetOwnProfile(
            CancellationToken cancellationToken = default)
        {
            var user = await _mediator.Send(
                new GetOwnProfileQuery(),
                cancellationToken);

            if (user is null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        // GET: api/users/{id}
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(
            string id,
            CancellationToken cancellationToken)
        {
            var user = await _mediator.Send(
                new GetUserByIdQuery(id),
                cancellationToken);

            if (user is null)
            {
                return NotFound();
            }

            if (User.IsInRole("Admin"))
            {
                await _auditLogger.LogAsync(
                    new AuditEvent(
                        AuditAction.UserProfileViewed,
                        AuditEntityTypes.User,
                        id),
                    cancellationToken);
            }

            return Ok(user);
        }

        // PUT: api/users/me
        // Self-service profile update. The target account is derived from the
        // authenticated identity; privileged fields cannot be changed here.
        [Authorize]
        [HttpPut("self")]
        public async Task<IActionResult> UpdateOwnProfile(
            [FromBody] UpdateOwnProfileRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdateOwnProfileCommand
            {
                Email = request.Email,
                Password = request.Password,
            };

            var user = await _mediator.Send(command, cancellationToken);

            await _auditLogger.LogAsync(
                new AuditEvent(
                    AuditAction.OwnProfileUpdated,
                    AuditEntityTypes.User,
                    user.UserId,
                    Details: AuditDetails.Create(
                        ("PasswordChanged", !string.IsNullOrWhiteSpace(request.Password)))),
                cancellationToken);

            return Ok(user);
        }

        // PUT: api/users/{id}
        // Administrative update of another user. Requires the Admin policy.
        [Authorize(Policy = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(
            string id,
            [FromBody] UpdateUserRequest request,
            CancellationToken cancellationToken)
        {
            var command = new AdminUpdateUserCommand
            {
                UserId = id,
                Email = request.Email,
                RoleId = request.RoleId,
                IsVerified = request.IsVerified,
                Password = request.Password,
            };

            var user = await _mediator.Send(command, cancellationToken);

            await _auditLogger.LogAsync(
                new AuditEvent(
                    AuditAction.UserUpdated,
                    AuditEntityTypes.User,
                    id,
                    Details: AuditDetails.Create(
                        ("RoleId", request.RoleId),
                        ("IsVerified", request.IsVerified),
                        ("PasswordChanged", !string.IsNullOrWhiteSpace(request.Password)))),
                cancellationToken);

            return Ok(user);
        }

        // DELETE: api/users/{id}
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(
            string id,
            CancellationToken cancellationToken)
        {
            await _mediator.Send(new DeleteUserCommand(id), cancellationToken);

            await _auditLogger.LogAsync(
                new AuditEvent(
                    AuditAction.UserDeleted,
                    AuditEntityTypes.User,
                    id),
                cancellationToken);

            return NoContent();
        }
    }
}