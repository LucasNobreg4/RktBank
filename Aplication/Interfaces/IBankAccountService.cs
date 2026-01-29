using Application.DTOs;

namespace Application.Interfaces
{
    public interface IBankAccountService
    {
        Task<BankAccountResponseDto> CreateAccountAsync(CreateBankAccountDto dto);
        Task<BankAccountDto?> GetAccountByIdAsync(string id);
        Task<BankAccountDto?> GetAccountByNumberAsync(string accountNumber);
        Task<List<BankAccountDto>> GetCustomerAccountsAsync(string customerId);
        Task<TransactionResponseDto> DepositAsync(DepositDto dto);
        Task<TransactionResponseDto> WithdrawAsync(WithdrawDto dto);
        Task<TransactionResponseDto> TransferAsync(TransferDto dto);
        Task<List<TransactionDto>> GetTransactionHistoryAsync(string accountId);
        Task<bool> DeactivateAccountAsync(string id);
    }
}
