namespace Application.DTOs
{
    public class WithdrawDto
    {
        public string AccountId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
