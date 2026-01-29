namespace Application.DTOs
{
    public class CustomerResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public CustomerDto? Customer { get; set; }
    }
}
