using Domain.Entities;

namespace Infrastructure.Interfaces
{
    public interface IBankAccountRepository
    {
        Task<BankAccount?> GetByIdAsync(string id);
        Task<BankAccount?> GetByAccountNumberAsync(string accountNumber);
        Task<List<BankAccount>> GetByCustomerIdAsync(string customerId);
        Task<BankAccount> CreateAsync(BankAccount account);
        Task<bool> UpdateAsync(BankAccount account);
        Task<bool> DeleteAsync(string id);
        Task<bool> DepositAsync(string accountId, decimal amount, string description);
        Task<bool> WithdrawAsync(string accountId, decimal amount, string description);
        Task<bool> TransferAsync(string fromAccountId, string toAccountId, decimal amount, string description);
        Task<List<Transaction>> GetTransactionHistoryAsync(string accountId);
    }
}
