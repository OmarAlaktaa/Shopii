using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopii.API.Models;
using Shopii.Application.Common.Auditing;
using Shopii.Application.Features.Users.Commands.AdminUpdateUser;
using Shopii.Application.Interfaces.Services;

namespace Shopii.API.Controllers
{
    [Authorize(Policy = "Admin")]
    [Route("api/admin/users")]
    [ApiController]
    public class AdminUsersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IAuditLogger _auditLogger;

        public AdminUsersController(
            IMediator mediator,
            IAuditLogger auditLogger)
        {
            _mediator = mediator;
            _auditLogger = auditLogger;
        }

        // PUT: api/admin/users/{id}
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
    }
}
