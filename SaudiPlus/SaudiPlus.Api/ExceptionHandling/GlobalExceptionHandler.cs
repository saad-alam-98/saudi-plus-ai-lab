using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;

namespace SaudiPlus.Api.ExceptionHandling
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly IProblemDetailsService _problemDetailsService;

        public GlobalExceptionHandler(IProblemDetailsService problemDetailsService)
        {
            _problemDetailsService = problemDetailsService;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext, 
            Exception exception, 
            CancellationToken cancellationToken
        )
        {
            var statusCode = exception switch
            {
                ArgumentException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            httpContext.Response.StatusCode = statusCode;

            return await _problemDetailsService.TryWriteAsync(
                new ProblemDetailsContext {
                    HttpContext = httpContext,
                    ProblemDetails = new ProblemDetails
                    {
                        Status = statusCode,
                        Title = statusCode == 400
                        ? "Invalid request"
                        : "Internal server error",
                        Detail = statusCode == 400
                        ? exception.Message
                        : "An unexpected error occurred."
                    }
                }
            );
        }
    }
}
