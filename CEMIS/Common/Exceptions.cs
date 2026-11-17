using System;

namespace CEMIS.Common;

public class AppUnauthorizedException : Exception
{
    public AppUnauthorizedException(string message) : base(message)
    {
    }
}

public class AppForbiddenException : Exception
{
    public AppForbiddenException(string message) : base(message)
    {
    }
}
