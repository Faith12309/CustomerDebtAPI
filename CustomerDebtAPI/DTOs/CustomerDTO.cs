namespace CustomerDebtAPI.DTOs
{
    public class CustomerDTO
    {
        public string FullName { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string ContactNumber { get; set; } = string.Empty;

        public string IdType { get; set; } = string.Empty;

        public string IdNumber { get; set; } = string.Empty;

        public string? IdImage { get; set; }
    }
}