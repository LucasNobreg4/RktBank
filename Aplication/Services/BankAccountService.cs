using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Interfaces;

namespace Application.Services
{
    public class BankAccountService : IBankAccountService
    {
        private readonly IBankAccountRepository _accountRepository;
        private readonly ICustomerRepository _customerRepository;

        public BankAccountService(
            IBankAccountRepository accountRepository,
            ICustomerRepository customerRepository)
        {
            _accountRepository = accountRepository;
            _customerRepository = customerRepository;
        }

        public async Task<BankAccountResponseDto> CreateAccountAsync(CreateBankAccountDto dto)
        {
            var customer = await _customerRepository.GetByIdAsync(dto.CustomerId);
            if (customer == null)
            {
                return new BankAccountResponseDto
                {
                    Success = false,
                    Message = "Cliente não encontrado."
                };
            }

            var account = new BankAccount
            {
                CustomerId = dto.CustomerId,
                Balance = 0
            };

            var createdAccount = await _accountRepository.CreateAsync(account);

            return new BankAccountResponseDto
            {
                Success = true,
                Message = "Conta criada com sucesso.",
                Account = MapToDto(createdAccount)
            };
        }

        public async Task<BankAccountDto?> GetAccountByIdAsync(string id)
        {
            var account = await _accountRepository.GetByIdAsync(id);
            return account != null ? MapToDto(account) : null;
        }

        public async Task<BankAccountDto?> GetAccountByNumberAsync(string accountNumber)
        {
            var account = await _accountRepository.GetByAccountNumberAsync(accountNumber);
            return account != null ? MapToDto(account) : null;
        }

        public async Task<List<BankAccountDto>> GetCustomerAccountsAsync(string customerId)
        {
            var accounts = await _accountRepository.GetByCustomerIdAsync(customerId);
            return accounts.Select(MapToDto).ToList();
        }

        public async Task<TransactionResponseDto> DepositAsync(DepositDto dto)
        {
            var success = await _accountRepository.DepositAsync(dto.AccountId, dto.Amount, dto.Description);

            if (!success)
            {
                return new TransactionResponseDto
                {
                    Success = false,
                    Message = "Falha ao realizar depósito."
                };
            }

            var account = await _accountRepository.GetByIdAsync(dto.AccountId);

            return new TransactionResponseDto
            {
                Success = true,
                Message = "Depósito realizado com sucesso.",
                NewBalance = account?.Balance
            };
        }

        public async Task<TransactionResponseDto> WithdrawAsync(WithdrawDto dto)
        {
            var success = await _accountRepository.WithdrawAsync(dto.AccountId, dto.Amount, dto.Description);

            if (!success)
            {
                return new TransactionResponseDto
                {
                    Success = false,
                    Message = "Falha ao realizar saque. Verifique o saldo."
                };
            }

            var account = await _accountRepository.GetByIdAsync(dto.AccountId);

            return new TransactionResponseDto
            {
                Success = true,
                Message = "Saque realizado com sucesso.",
                NewBalance = account?.Balance
            };
        }

        public async Task<TransactionResponseDto> TransferAsync(TransferDto dto)
        {
            var success = await _accountRepository.TransferAsync(
                dto.FromAccountId,
                dto.ToAccountId,
                dto.Amount,
                dto.Description);

            if (!success)
            {
                return new TransactionResponseDto
                {
                    Success = false,
                    Message = "Falha ao realizar transferência. Verifique o saldo."
                };
            }

            var account = await _accountRepository.GetByIdAsync(dto.FromAccountId);

            return new TransactionResponseDto
            {
                Success = true,
                Message = "Transferência realizada com sucesso.",
                NewBalance = account?.Balance
            };
        }

        public async Task<List<TransactionDto>> GetTransactionHistoryAsync(string accountId)
        {
            var transactions = await _accountRepository.GetTransactionHistoryAsync(accountId);
            return transactions.Select(t => new TransactionDto
            {
                Id = t.Id,
                Type = t.Type.ToString(),
                Amount = t.Amount,
                BalanceAfter = t.BalanceAfter,
                Description = t.Description,
                CreatedAt = t.CreatedAt
            }).ToList();
        }

        public async Task<bool> DeactivateAccountAsync(string id)
        {
            return await _accountRepository.DeleteAsync(id);
        }

        private static BankAccountDto MapToDto(BankAccount account)
        {
            return new BankAccountDto
            {
                Id = account.Id,
                AccountNumber = account.AccountNumber,
                Balance = account.Balance,
                CustomerId = account.CustomerId,
                CreatedAt = account.CreatedAt,
                UpdatedAt = account.UpdatedAt,
                IsActive = account.IsActive
            };
        }
    }
}
