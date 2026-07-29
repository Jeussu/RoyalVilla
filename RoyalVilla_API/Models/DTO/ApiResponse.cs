namespace RoyalVilla_API.Models.DTO
{
    public class ApiResponse<TData>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public TData? Data { get; set; }
        public object? Error { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
