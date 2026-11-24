using Microsoft.AspNetCore.Mvc;
using SaudiPlus.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace SaudiPlus.Api.ExceptionHandling
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly IProblemDetailsService _problemDetailsService;
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(
            IProblemDetailsService problemDetailsService, 
            ILogger<GlobalExceptionHandler> logger
        )
        {
            _problemDetailsService = problemDetailsService;
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext, 
            Exception exception, 
            CancellationToken cancellationToken
        )
        {
            var statusCode = exception switch
            {
                DomainValidationException => StatusCodes.Status400BadRequest,

                _ => StatusCodes.Status500InternalServerError
            };

            httpContext.Response.StatusCode = statusCode;

            if (exception is DomainValidationException)
            {
                _logger.LogWarning(
                    "Domain validation failed: {ValidationMessage}",
                    exception.Message
                );
            }
            else
            {
                _logger.LogError(
                    exception,
                    "An unexpected error occurred."
                );
            }

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
