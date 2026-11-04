using MediatR;

namespace Shopii.Application.Features.Users.Commands.DeleteUser
{
    public sealed record DeleteUserCommand(string UserId) : IRequest<bool>;
}
