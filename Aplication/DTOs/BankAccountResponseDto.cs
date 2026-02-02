namespace Application.DTOs
{
    public class BankAccountResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public BankAccountDto? Account { get; set; }
    }
}
