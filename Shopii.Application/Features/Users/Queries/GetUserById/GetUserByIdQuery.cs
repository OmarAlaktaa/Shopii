using MediatR;
using Shopii.Application.Features.Users.DTOs;

namespace Shopii.Application.Features.Users.Queries.GetUserById;

/// <summary>
/// this function works as a query to get a user by their ID.
/// It implements the IRequest interface from MediatR, 
/// which allows it to be handled by a corresponding query handler. 
/// The query takes UserId of the user to be retrieved. 
/// The expected return type is UserDTO?, 
/// indicating that the result may be null if no user is found with the given ID.
/// </summary>
public record GetUserByIdQuery(string UserId)
    : IRequest<UserDTO?>;