using MediatR;
using Shopii.Application.Features.Users.DTOs;

namespace Shopii.Application.Features.Users.Queries.GetUserByEmail
{
    public record GetUserByEmailQuery(string Email)
        : IRequest<UserDTO?>;
}