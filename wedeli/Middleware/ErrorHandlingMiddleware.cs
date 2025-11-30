using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using wedeli.Exceptions;
using wedeli.Models.Response;

namespace wedeli.Middleware
{
    /// <summary>
    /// Global exception handling middleware
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public ErrorHandlingMiddleware(
            RequestDelegate next,
            ILogger<ErrorHandlingMiddleware> logger,
            IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var response = new ErrorResponse();

            switch (exception)
            {
                case AppException appException:
                    context.Response.StatusCode = appException.StatusCode;
                    response.Message = appException.Message;
                    response.ErrorCode = appException.ErrorCode;
                    response.StatusCode = appException.StatusCode;

                    // Include validation errors n?u có
                    if (appException.Errors != null && appException.Errors.Count > 0)
                    {
                        response.Details = JsonSerializer.Serialize(appException.Errors);
                    }
                    break;

                case UnauthorizedAccessException _:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response.Message = "Unauthorized access.";
                    response.ErrorCode = "UNAUTHORIZED";
                    response.StatusCode = 401;
                    break;

                case ArgumentNullException argNullEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = $"Required parameter '{argNullEx.ParamName}' is missing.";
                    response.ErrorCode = "MISSING_PARAMETER";
                    response.StatusCode = 400;
                    break;

                case ArgumentException argEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = argEx.Message;
                    response.ErrorCode = "INVALID_ARGUMENT";
                    response.StatusCode = 400;
                    break;

                case InvalidOperationException invOpEx:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = invOpEx.Message;
                    response.ErrorCode = "INVALID_OPERATION";
                    response.StatusCode = 400;
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.Message = "An internal server error occurred.";
                    response.ErrorCode = "INTERNAL_SERVER_ERROR";
                    response.StatusCode = 500;
                    break;
            }

            // Include stack trace only in Development environment
            if (_environment.IsDevelopment())
            {
                response.StackTrace = exception.StackTrace;

                if (exception.InnerException != null)
                {
                    response.Details = $"Inner Exception: {exception.InnerException.Message}";
                }
            }

            response.Timestamp = DateTime.UtcNow;

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}