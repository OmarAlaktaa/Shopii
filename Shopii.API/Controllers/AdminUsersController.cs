using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shopii.API.Models;
using Shopii.Application.Features.Users.Commands.AdminUpdateUser;

namespace Shopii.API.Controllers
{
    [Authorize(Policy = "Admin")]
    [Route("api/admin/users")]
    [ApiController]
    public class AdminUsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminUsersController(IMediator mediator)
        {
            _mediator = mediator;
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
            return Ok(user);
        }
    }
}