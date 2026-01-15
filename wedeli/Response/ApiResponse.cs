using System;
using System.Collections.Generic;

namespace wedeli.Models.Response
{
    /// <summary>
    /// Standard API response wrapper for all endpoints
    /// </summary>
    /// <typeparam name="T">Type of data being returned</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Indicates if the request was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Human-readable message describing the result
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// The actual data payload
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// List of error messages (if any)
        /// </summary>
        public List<string> Errors { get; set; }

        /// <summary>
        /// Timestamp when the response was generated
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// HTTP status code
        /// </summary>
        public int StatusCode { get; set; }

        public ApiResponse()
        {
            Timestamp = DateTime.UtcNow;
            Errors = new List<string>();
        }

        // Factory methods for common responses

        /// <summary>
        /// Create a successful response with data
        /// </summary>
        public static ApiResponse<T> SuccessResponse(T data, string message = "Success", int statusCode = 200)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                StatusCode = statusCode,
                Errors = new List<string>()
            };
        }

        /// <summary>
        /// Create a successful response without data
        /// </summary>
        public static ApiResponse<T> SuccessResponse(string message = "Success", int statusCode = 200)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = default(T),
                StatusCode = statusCode,
                Errors = new List<string>()
            };
        }

        /// <summary>
        /// Create an error response
        /// </summary>
        public static ApiResponse<T> ErrorResponse(string message, int statusCode = 400, List<string> errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default(T),
                StatusCode = statusCode,
                Errors = errors ?? new List<string>()
            };
        }

        /// <summary>
        /// Create an error response with single error
        /// </summary>
        public static ApiResponse<T> ErrorResponse(string message, string error, int statusCode = 400)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default(T),
                StatusCode = statusCode,
                Errors = new List<string> { error }
            };
        }
    }

    /// <summary>
    /// Non-generic version for responses without data
    /// </summary>
    public class ApiResponse : ApiResponse<object>
    {
        public static new ApiResponse SuccessResponse(string message = "Success", int statusCode = 200)
        {
            return new ApiResponse
            {
                Success = true,
                Message = message,
                StatusCode = statusCode,
                Errors = new List<string>()
            };
        }

        public static new ApiResponse ErrorResponse(string message, int statusCode = 400, List<string> errors = null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                StatusCode = statusCode,
                Errors = errors ?? new List<string>()
            };
        }
    }

    /// <summary>
    /// Paginated response for list endpoints
    /// </summary>
    /// <typeparam name="T">Type of items in the list</typeparam>
    public class PagedResponse<T>
    {
        /// <summary>
        /// List of items for current page
        /// </summary>
        public List<T> Items { get; set; }

        /// <summary>
        /// Total number of items across all pages
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Current page number (1-based)
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of pages
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Indicates if there is a previous page
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;

        /// <summary>
        /// Indicates if there is a next page
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;

        public PagedResponse()
        {
            Items = new List<T>();
        }

        public PagedResponse(List<T> items, int totalItems, int pageNumber, int pageSize)
        {
            Items = items ?? new List<T>();
            TotalItems = totalItems;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        }

        /// <summary>
        /// Create a paged response with metadata
        /// </summary>
        public static PagedResponse<T> Create(List<T> items, int totalItems, int pageNumber, int pageSize)
        {
            return new PagedResponse<T>(items, totalItems, pageNumber, pageSize);
        }
    }

    /// <summary>
    /// Validation error detail
    /// </summary>
    public class ValidationError
    {
        /// <summary>
        /// Field name that failed validation
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// Validation error message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Attempted value
        /// </summary>
        public object AttemptedValue { get; set; }

        public ValidationError() { }

        public ValidationError(string field, string message, object attemptedValue = null)
        {
            Field = field;
            Message = message;
            AttemptedValue = attemptedValue;
        }
    }

    /// <summary>
    /// API response for validation errors
    /// </summary>
    public class ValidationErrorResponse
    {
        public bool Success => false;
        public string Message { get; set; }
        public List<ValidationError> Errors { get; set; }
        public DateTime Timestamp { get; set; }
        public int StatusCode => 400;

        public ValidationErrorResponse()
        {
            Message = "Validation failed";
            Errors = new List<ValidationError>();
            Timestamp = DateTime.UtcNow;
        }

        public ValidationErrorResponse(string message, List<ValidationError> errors)
        {
            Message = message;
            Errors = errors ?? new List<ValidationError>();
            Timestamp = DateTime.UtcNow;
        }

        public static ValidationErrorResponse Create(List<ValidationError> errors, string message = "Validation failed")
        {
            return new ValidationErrorResponse(message, errors);
        }
    }

    /// <summary>
    /// API response for exceptions
    /// </summary>
    public class ErrorResponse
    {
        public bool Success => false;
        public string Message { get; set; }
        public string ErrorCode { get; set; }
        public string Details { get; set; }
        public string StackTrace { get; set; }
        public DateTime Timestamp { get; set; }
        public int StatusCode { get; set; }

        public ErrorResponse()
        {
            Timestamp = DateTime.UtcNow;
        }

        public ErrorResponse(string message, int statusCode, string errorCode = null, string details = null, string stackTrace = null)
        {
            Message = message;
            StatusCode = statusCode;
            ErrorCode = errorCode;
            Details = details;
            StackTrace = stackTrace;
            Timestamp = DateTime.UtcNow;
        }

        public static ErrorResponse Create(string message, int statusCode = 500, string errorCode = null, string details = null, string stackTrace = null)
        {
            return new ErrorResponse(message, statusCode, errorCode, details, stackTrace);
        }
    }

    /// <summary>
    /// Query parameters for pagination
    /// </summary>
    public class PaginationParams
    {
        private const int MaxPageSize = 100;
        private int _pageSize = 20;

        /// <summary>
        /// Page number (1-based)
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Number of items per page (max 100)
        /// </summary>
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }

        /// <summary>
        /// Calculate skip count for database query
        /// </summary>
        public int Skip => (PageNumber - 1) * PageSize;

        /// <summary>
        /// Get take count for database query
        /// </summary>
        public int Take => PageSize;
    }

    /// <summary>
    /// Query parameters with search and filtering
    /// </summary>
    public class QueryParams : PaginationParams
    {
        /// <summary>
        /// Search keyword
        /// </summary>
        public string SearchTerm { get; set; }

        /// <summary>
        /// Sort field name
        /// </summary>
        public string SortBy { get; set; }

        /// <summary>
        /// Sort order: asc or desc
        /// </summary>
        public string SortOrder { get; set; } = "asc";

        /// <summary>
        /// Check if sorting is descending
        /// </summary>
        public bool IsSortDescending => SortOrder?.ToLower() == "desc";
    }
}