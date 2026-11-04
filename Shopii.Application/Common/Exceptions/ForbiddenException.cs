namespace Shopii.Application.Common.Exceptions;

public class ForbiddenException : UnauthorizedException
{
    public ForbiddenException(string message) : base(message)
    {
    }
}