namespace wedeli.Models.DTO{
public class ApiResponse<T>
{
    /// <summary>
    /// Thành công hay thất bại
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Message mô tả kết quả
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Data trả về (nếu có)
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Errors chi tiết (nếu có)
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; set; }

    /// <summary>
    /// Timestamp
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
}