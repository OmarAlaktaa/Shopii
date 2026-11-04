using Shopii.Domain.Entities.Users;

namespace Shopii.Application.Features.Users.DTOs
{
    public static class UserMappingExtensions
    {
        public static UserDTO ToDTO(this User user)
        {
            return new UserDTO
            {
                UserId = user.UserId,
                Email = user.Email,
                RoleId = user.RoleId,
                IsVerified = user.IsVerified,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
    }
}
