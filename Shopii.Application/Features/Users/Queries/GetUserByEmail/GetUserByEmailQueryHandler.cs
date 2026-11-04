using MediatR;
using Shopii.Application.Features.Users.DTOs;
using Shopii.Application.Interfaces.Persistence;

namespace Shopii.Application.Features.Users.Queries.GetUserByEmail;

public class GetUserByEmailQueryHandler
    : IRequestHandler<GetUserByEmailQuery, UserDTO?>
{
    private readonly IUserRepository _userRepository;

    public GetUserByEmailQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDTO?> Handle(
        GetUserByEmailQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(
            request.Email,
            cancellationToken);

        if (user is null)
            return null;

        return user.ToDTO();
    }
}