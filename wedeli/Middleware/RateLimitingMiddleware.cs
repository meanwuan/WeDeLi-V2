using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;
using wedeli.Models.Response;

namespace wedeli.Middleware
{
    /// <summary>
    /// Middleware để rate limiting - giới hạn số requests từ một IP
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly ILogger<RateLimitingMiddleware> _logger;

        // Cấu hình rate limiting
        private const int MaxRequestsPerMinute = 60;
        private const int MaxRequestsPerHour = 1000;

        public RateLimitingMiddleware(
            RequestDelegate next,
            IMemoryCache cache,
            ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _cache = cache;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientIp = GetClientIpAddress(context);

            // Skip rate limiting cho localhost trong development
            if (IsLocalhost(clientIp) && context.Request.Host.Host == "localhost")
            {
                await _next(context);
                return;
            }

            var minuteKey = $"rate_limit_minute_{clientIp}";
            var hourKey = $"rate_limit_hour_{clientIp}";

            // Kiểm tra rate limit per minute
            var minuteCount = _cache.GetOrCreate(minuteKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                return 0;
            });

            // Kiểm tra rate limit per hour
            var hourCount = _cache.GetOrCreate(hourKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                return 0;
            });

            if (minuteCount >= MaxRequestsPerMinute)
            {
                _logger.LogWarning(
                    "Rate limit exceeded (per minute) for IP: {IpAddress}. Requests: {Count}",
                    clientIp,
                    minuteCount
                );

                await SendRateLimitResponseAsync(context, "Too many requests per minute. Please try again later.");
                return;
            }

            if (hourCount >= MaxRequestsPerHour)
            {
                _logger.LogWarning(
                    "Rate limit exceeded (per hour) for IP: {IpAddress}. Requests: {Count}",
                    clientIp,
                    hourCount
                );

                await SendRateLimitResponseAsync(context, "Too many requests per hour. Please try again later.");
                return;
            }

            // Increment counters
            _cache.Set(minuteKey, minuteCount + 1, TimeSpan.FromMinutes(1));
            _cache.Set(hourKey, hourCount + 1, TimeSpan.FromHours(1));

            // Add rate limit headers
            context.Response.Headers.Add("X-RateLimit-Limit-Minute", MaxRequestsPerMinute.ToString());
            context.Response.Headers.Add("X-RateLimit-Remaining-Minute", (MaxRequestsPerMinute - minuteCount - 1).ToString());
            context.Response.Headers.Add("X-RateLimit-Limit-Hour", MaxRequestsPerHour.ToString());
            context.Response.Headers.Add("X-RateLimit-Remaining-Hour", (MaxRequestsPerHour - hourCount - 1).ToString());

            await _next(context);
        }

        private string GetClientIpAddress(HttpContext context)
        {
            // Kiểm tra X-Forwarded-For header (nếu đằng sau proxy)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                var ips = forwardedFor.Split(',');
                return ips[0].Trim();
            }

            // Kiểm tra X-Real-IP header
            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            // Fallback to RemoteIpAddress
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private bool IsLocalhost(string ipAddress)
        {
            return ipAddress == "::1" ||
                   ipAddress == "127.0.0.1" ||
                   ipAddress.StartsWith("192.168.") ||
                   ipAddress.StartsWith("10.") ||
                   ipAddress == "unknown";
        }

        private async Task SendRateLimitResponseAsync(HttpContext context, string message)
        {
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.ContentType = "application/json";

            var response = ApiResponse.ErrorResponse(message, 429);
            var json = System.Text.Json.JsonSerializer.Serialize(response);

            await context.Response.WriteAsync(json);
        }
    }
}