namespace Domain.Entities
{
    public class BankAccount
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string AccountNumber { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public Customer Customer { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
