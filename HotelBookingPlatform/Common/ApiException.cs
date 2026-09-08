using System.Net;

namespace HotelBookingPlatform.Common;

public abstract class ApiException : Exception
{
    public HttpStatusCode StatusCode { get; }

    protected ApiException(HttpStatusCode statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
    }
}

public class NotFoundException : ApiException
{
    public NotFoundException(string message) : base(HttpStatusCode.NotFound, message) { }
}

public class BadRequestException : ApiException
{
    public BadRequestException(string message) : base(HttpStatusCode.BadRequest, message) { }
}

public class ConflictException : ApiException
{
    public ConflictException(string message) : base(HttpStatusCode.Conflict, message) { }
}

public class UnauthorizedException : ApiException
{
    public UnauthorizedException(string message) : base(HttpStatusCode.Unauthorized, message) { }
}
