using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class BankAccountRepository : IBankAccountRepository
    {
        private readonly ApplicationDbContext _context;

        public BankAccountRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BankAccount?> GetByIdAsync(string id)
        {
            return await _context.BankAccounts
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<BankAccount?> GetByAccountNumberAsync(string accountNumber)
        {
            return await _context.BankAccounts
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);
        }

        public async Task<List<BankAccount>> GetByCustomerIdAsync(string customerId)
        {
            return await _context.BankAccounts
                .Where(a => a.CustomerId == customerId)
                .ToListAsync();
        }

        public async Task<BankAccount> CreateAsync(BankAccount account)
        {
            account.AccountNumber = await GenerateAccountNumberAsync();
            _context.BankAccounts.Add(account);
            await _context.SaveChangesAsync();
            return account;
        }

        public async Task<bool> UpdateAsync(BankAccount account)
        {
            account.UpdatedAt = DateTime.UtcNow;
            _context.BankAccounts.Update(account);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var account = await GetByIdAsync(id);
            if (account == null) return false;

            account.IsActive = false;
            account.UpdatedAt = DateTime.UtcNow;
            return await UpdateAsync(account);
        }

        public async Task<bool> DepositAsync(string accountId, decimal amount, string description)
        {
            if (amount <= 0) return false;

            var account = await GetByIdAsync(accountId);
            if (account == null || !account.IsActive) return false;

            account.Balance += amount;
            account.UpdatedAt = DateTime.UtcNow;

            var transaction = new Transaction
            {
                BankAccountId = accountId,
                Type = TransactionType.Deposit,
                Amount = amount,
                BalanceAfter = account.Balance,
                Description = description
            };

            _context.Transactions.Add(transaction);
            _context.BankAccounts.Update(account);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> WithdrawAsync(string accountId, decimal amount, string description)
        {
            if (amount <= 0) return false;

            var account = await GetByIdAsync(accountId);
            if (account == null || !account.IsActive) return false;

            if (account.Balance < amount) return false;

            account.Balance -= amount;
            account.UpdatedAt = DateTime.UtcNow;

            var transaction = new Transaction
            {
                BankAccountId = accountId,
                Type = TransactionType.Withdrawal,
                Amount = amount,
                BalanceAfter = account.Balance,
                Description = description
            };

            _context.Transactions.Add(transaction);
            _context.BankAccounts.Update(account);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> TransferAsync(string fromAccountId, string toAccountId, decimal amount, string description)
        {
            if (amount <= 0) return false;

            var fromAccount = await GetByIdAsync(fromAccountId);
            var toAccount = await GetByIdAsync(toAccountId);

            if (fromAccount == null || !fromAccount.IsActive ||
                toAccount == null || !toAccount.IsActive)
                return false;

            if (fromAccount.Balance < amount) return false;

            fromAccount.Balance -= amount;
            fromAccount.UpdatedAt = DateTime.UtcNow;

            toAccount.Balance += amount;
            toAccount.UpdatedAt = DateTime.UtcNow;

            var withdrawTransaction = new Transaction
            {
                BankAccountId = fromAccountId,
                Type = TransactionType.Transfer,
                Amount = -amount,
                BalanceAfter = fromAccount.Balance,
                Description = $"Transferência para {toAccount.AccountNumber}: {description}"
            };

            var depositTransaction = new Transaction
            {
                BankAccountId = toAccountId,
                Type = TransactionType.Transfer,
                Amount = amount,
                BalanceAfter = toAccount.Balance,
                Description = $"Transferência de {fromAccount.AccountNumber}: {description}"
            };

            _context.Transactions.AddRange(withdrawTransaction, depositTransaction);
            _context.BankAccounts.UpdateRange(fromAccount, toAccount);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<Transaction>> GetTransactionHistoryAsync(string accountId)
        {
            return await _context.Transactions
                .Where(t => t.BankAccountId == accountId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        private async Task<string> GenerateAccountNumberAsync()
        {
            string accountNumber;
            bool exists;

            do
            {
                accountNumber = Random.Shared.Next(10000000, 99999999).ToString();
                exists = await _context.BankAccounts
                    .AnyAsync(a => a.AccountNumber == accountNumber);
            } while (exists);

            return accountNumber;
        }
    }
}
