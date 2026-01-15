using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace wedeli.Middleware
{
    /// <summary>
    /// Middleware để theo dõi performance của các requests
    /// Cảnh báo nếu request xử lý chậm
    /// </summary>
    public class PerformanceLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PerformanceLoggingMiddleware> _logger;
        private const int SlowRequestThresholdMs = 3000; // 3 seconds

        public PerformanceLoggingMiddleware(RequestDelegate next, ILogger<PerformanceLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestPath = context.Request.Path.Value;
            var requestMethod = context.Request.Method;

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                var elapsedMs = stopwatch.ElapsedMilliseconds;

                // Log performance metrics
                LogPerformance(requestMethod, requestPath, elapsedMs, context.Response.StatusCode);

                // Cảnh báo nếu request quá chậm
                if (elapsedMs > SlowRequestThresholdMs)
                {
                    _logger.LogWarning(
                        "SLOW REQUEST detected! {Method} {Path} took {ElapsedMs}ms (Status: {StatusCode})",
                        requestMethod,
                        requestPath,
                        elapsedMs,
                        context.Response.StatusCode
                    );
                }
            }
        }

        private void LogPerformance(string method, string path, long elapsedMs, int statusCode)
        {
            var logLevel = statusCode >= 500 ? LogLevel.Error :
                          statusCode >= 400 ? LogLevel.Warning :
                          elapsedMs > SlowRequestThresholdMs ? LogLevel.Warning :
                          LogLevel.Information;

            _logger.Log(
                logLevel,
                "Performance: {Method} {Path} completed in {ElapsedMs}ms with status {StatusCode}",
                method,
                path,
                elapsedMs,
                statusCode
            );
        }
    }
}