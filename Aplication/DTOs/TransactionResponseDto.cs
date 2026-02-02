namespace Application.DTOs
{
    public class TransactionResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal? NewBalance { get; set; }
    }
}
