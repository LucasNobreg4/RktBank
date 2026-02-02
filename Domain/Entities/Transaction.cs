using Domain.Enums;

namespace Domain.Entities
{
    public class Transaction
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string BankAccountId { get; set; } = string.Empty;
        public BankAccount BankAccount { get; set; } = null!;
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
