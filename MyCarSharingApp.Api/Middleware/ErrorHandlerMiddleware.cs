using Microsoft.AspNetCore.Mvc;
using MyCarSharingApp.Application.Exceptions;
using System.Net;
using System.Text.Json;

namespace MyCarSharingApp.Api.Middleware
{
    /// <summary>
    /// Middleware to catch exceptions and convert them to standardized ProblemDetails responses.
    /// </summary>
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext ctx)
        {
            try
            {
                await _next(ctx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");

                int statusCode = (int)HttpStatusCode.InternalServerError;
                string title = "An unexpected error occurred.";

                if (ex is EntityNotFoundException)
                {
                    statusCode = (int)HttpStatusCode.NotFound;
                    title = "Resource not found.";
                }
                else if (ex is ArgumentException)
                {
                    statusCode = (int)HttpStatusCode.BadRequest;
                    title = "Bad request.";
                }

                ctx.Response.ContentType = "application/problem+json";
                ctx.Response.StatusCode = statusCode;

                var problemDetails = new ProblemDetails
                {
                    Status = statusCode,
                    Title = title,
                    Detail = ex.Message,
                    Instance = ctx.Request.Path
                };

                await ctx.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
            }
        }
    }
}