using System;
using System.Collections.Generic;

namespace wedeli.Exceptions
{
    /// <summary>
    /// Base exception cho tất cả custom exceptions
    /// </summary>
    public class AppException : Exception
    {
        public int StatusCode { get; set; }
        public string ErrorCode { get; set; }
        public Dictionary<string, string[]> Errors { get; set; }

        public AppException(string message, int statusCode = 500, string errorCode = null)
            : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
            Errors = new Dictionary<string, string[]>();
        }

        public AppException(string message, Exception innerException)
            : base(message, innerException)
        {
            StatusCode = 500;
        }
    }

    /// <summary>
    /// 400 Bad Request - Request không hợp lệ
    /// </summary>
    public class BadRequestException : AppException
    {
        public BadRequestException(string message, string errorCode = "BAD_REQUEST")
            : base(message, 400, errorCode)
        {
        }

        public BadRequestException(string message, Dictionary<string, string[]> errors)
            : base(message, 400, "VALIDATION_ERROR")
        {
            Errors = errors;
        }
    }

    /// <summary>
    /// 401 Unauthorized - Chưa đăng nhập hoặc token không hợp lệ
    /// </summary>
    public class UnauthorizedException : AppException
    {
        public UnauthorizedException(string message = "Unauthorized. Please login to continue.")
            : base(message, 401, "UNAUTHORIZED")
        {
        }
    }

    /// <summary>
    /// 403 Forbidden - Không có quyền truy cập
    /// </summary>
    public class ForbiddenException : AppException
    {
        public ForbiddenException(string message = "You do not have permission to access this resource.")
            : base(message, 403, "FORBIDDEN")
        {
        }
    }

    /// <summary>
    /// 404 Not Found - Không tìm thấy resource
    /// </summary>
    public class NotFoundException : AppException
    {
        public NotFoundException(string resourceName, object key)
            : base($"{resourceName} with id '{key}' was not found.", 404, "NOT_FOUND")
        {
        }

        public NotFoundException(string message)
            : base(message, 404, "NOT_FOUND")
        {
        }
    }

    /// <summary>
    /// 409 Conflict - Xung đột dữ liệu (duplicate, etc.)
    /// </summary>
    public class ConflictException : AppException
    {
        public ConflictException(string message)
            : base(message, 409, "CONFLICT")
        {
        }

        public ConflictException(string resourceName, string field, object value)
            : base($"{resourceName} with {field} '{value}' already exists.", 409, "DUPLICATE")
        {
        }
    }

    /// <summary>
    /// 422 Unprocessable Entity - Validation failed
    /// </summary>
    public class ValidationException : AppException
    {
        public ValidationException(Dictionary<string, string[]> errors)
            : base("One or more validation errors occurred.", 422, "VALIDATION_ERROR")
        {
            Errors = errors;
        }

        public ValidationException(string field, string message)
            : base("Validation failed.", 422, "VALIDATION_ERROR")
        {
            Errors = new Dictionary<string, string[]>
            {
                { field, new[] { message } }
            };
        }
    }

    /// <summary>
    /// 500 Internal Server Error - Lỗi server
    /// </summary>
    public class InternalServerException : AppException
    {
        public InternalServerException(string message = "An internal server error occurred.")
            : base(message, 500, "INTERNAL_SERVER_ERROR")
        {
        }

        public InternalServerException(string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = "INTERNAL_SERVER_ERROR";
        }
    }

    /// <summary>
    /// 503 Service Unavailable - Service tạm thời không khả dụng
    /// </summary>
    public class ServiceUnavailableException : AppException
    {
        public ServiceUnavailableException(string serviceName)
            : base($"Service '{serviceName}' is temporarily unavailable. Please try again later.", 503, "SERVICE_UNAVAILABLE")
        {
        }
    }

    // ============================================
    // BUSINESS LOGIC EXCEPTIONS
    // ============================================

    /// <summary>
    /// Vehicle overload exception - Xe quá tải
    /// </summary>
    public class VehicleOverloadException : AppException
    {
        public VehicleOverloadException(string vehiclePlate, decimal currentWeight, decimal maxWeight)
            : base(
                $"Vehicle {vehiclePlate} is overloaded. Current: {currentWeight}kg, Max: {maxWeight}kg",
                400,
                "VEHICLE_OVERLOAD"
            )
        {
        }
    }

    /// <summary>
    /// Order cannot be cancelled exception
    /// </summary>
    public class OrderCannotBeCancelledException : AppException
    {
        public OrderCannotBeCancelledException(string orderStatus)
            : base(
                $"Order cannot be cancelled because it is already {orderStatus}.",
                400,
                "ORDER_CANNOT_BE_CANCELLED"
            )
        {
        }
    }

    /// <summary>
    /// Invalid order status transition
    /// </summary>
    public class InvalidOrderStatusException : AppException
    {
        public InvalidOrderStatusException(string currentStatus, string newStatus)
            : base(
                $"Cannot change order status from '{currentStatus}' to '{newStatus}'.",
                400,
                "INVALID_STATUS_TRANSITION"
            )
        {
        }
    }

    /// <summary>
    /// Driver not available exception
    /// </summary>
    public class DriverNotAvailableException : AppException
    {
        public DriverNotAvailableException(int driverId)
            : base(
                $"Driver with ID {driverId} is not available for assignment.",
                400,
                "DRIVER_NOT_AVAILABLE"
            )
        {
        }
    }

    /// <summary>
    /// Insufficient COD balance exception
    /// </summary>
    public class InsufficientCodBalanceException : AppException
    {
        public InsufficientCodBalanceException(decimal required, decimal available)
            : base(
                $"Insufficient COD balance. Required: {required}, Available: {available}",
                400,
                "INSUFFICIENT_COD_BALANCE"
            )
        {
        }
    }

    /// <summary>
    /// Payment already processed exception
    /// </summary>
    public class PaymentAlreadyProcessedException : AppException
    {
        public PaymentAlreadyProcessedException(int paymentId)
            : base(
                $"Payment with ID {paymentId} has already been processed.",
                409,
                "PAYMENT_ALREADY_PROCESSED"
            )
        {
        }
    }

    /// <summary>
    /// Customer not regular exception - Khách hàng chưa phải khách quen
    /// </summary>
    public class CustomerNotRegularException : AppException
    {
        public CustomerNotRegularException()
            : base(
                "This feature is only available for regular customers.",
                403,
                "CUSTOMER_NOT_REGULAR"
            )
        {
        }
    }

    /// <summary>
    /// Route not available exception
    /// </summary>
    public class RouteNotAvailableException : AppException
    {
        public RouteNotAvailableException(int routeId)
            : base(
                $"Route with ID {routeId} is not available at this time.",
                400,
                "ROUTE_NOT_AVAILABLE"
            )
        {
        }
    }
}