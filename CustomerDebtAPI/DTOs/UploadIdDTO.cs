using Microsoft.AspNetCore.Http;

namespace CustomerDebtAPI.DTOs
{
    public class UploadIdDTO
    {
        public IFormFile File { get; set; } = null!;
    }
}