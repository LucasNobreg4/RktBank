using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RktBank.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BankAccountController : ControllerBase
    {
        private readonly IBankAccountService _bankAccountService;

        public BankAccountController(IBankAccountService bankAccountService)
        {
            _bankAccountService = bankAccountService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] CreateBankAccountDto dto)
        {
            var result = await _bankAccountService.CreateAccountAsync(dto);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccountById(string id)
        {
            var account = await _bankAccountService.GetAccountByIdAsync(id);
            if (account == null)
                return NotFound();

            return Ok(account);
        }

        [HttpGet("number/{accountNumber}")]
        public async Task<IActionResult> GetAccountByNumber(string accountNumber)
        {
            var account = await _bankAccountService.GetAccountByNumberAsync(accountNumber);
            if (account == null)
                return NotFound();

            return Ok(account);
        }

        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetCustomerAccounts(string customerId)
        {
            var accounts = await _bankAccountService.GetCustomerAccountsAsync(customerId);
            return Ok(accounts);
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositDto dto)
        {
            var result = await _bankAccountService.DepositAsync(dto);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] WithdrawDto dto)
        {
            var result = await _bankAccountService.WithdrawAsync(dto);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("transfer")]
        public async Task<IActionResult> Transfer([FromBody] TransferDto dto)
        {
            var result = await _bankAccountService.TransferAsync(dto);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("{accountId}/transactions")]
        public async Task<IActionResult> GetTransactionHistory(string accountId)
        {
            var transactions = await _bankAccountService.GetTransactionHistoryAsync(accountId);
            return Ok(transactions);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeactivateAccount(string id)
        {
            var success = await _bankAccountService.DeactivateAccountAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}
