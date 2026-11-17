using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CEMIS.Common;

public class ApiExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var ex = context.Exception;

        if (ex is AppUnauthorizedException)
        {
            context.Result = new ObjectResult(ApiResponse<object>.Fail(ex.Message, 401))
            {
                StatusCode = 401
            };
            context.ExceptionHandled = true;
        }
        else if (ex is AppForbiddenException || ex is UnauthorizedAccessException)
        {
            // UnauthorizedAccessException trong business logic thường tương đương 403 Forbidden
            context.Result = new ObjectResult(ApiResponse<object>.Fail(ex.Message, 403))
            {
                StatusCode = 403
            };
            context.ExceptionHandled = true;
        }
        else if (ex is KeyNotFoundException)
        {
            context.Result = new ObjectResult(ApiResponse<object>.Fail(ex.Message, 404))
            {
                StatusCode = 404
            };
            context.ExceptionHandled = true;
        }
        else if (ex is InvalidOperationException || ex is ArgumentException)
        {
            context.Result = new ObjectResult(ApiResponse<object>.Fail(ex.Message, 400))
            {
                StatusCode = 400
            };
            context.ExceptionHandled = true;
        }
    }
}
