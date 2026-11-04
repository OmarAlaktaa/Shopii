using MediatR;
using Shopii.Application.Features.Users.DTOs;

namespace Shopii.Application.Features.Users.Queries.GetOwnProfile
{
    public sealed record GetOwnProfileQuery : IRequest<UserDTO?>;
}