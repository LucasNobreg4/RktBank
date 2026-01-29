using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Email já está em uso."
                };
            }

            var user = new ApplicationUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FullName = registerDto.FullName,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }

            return new AuthResponseDto
            {
                Success = true,
                Message = "Usuário registrado com sucesso.",
                UserId = user.Id,
                Email = user.Email
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Email ou senha inválidos."
                };
            }

            var result = await _signInManager.PasswordSignInAsync(
                user, 
                loginDto.Password, 
                isPersistent: false, 
                lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Conta bloqueada. Tente novamente mais tarde."
                    };
                }

                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Email ou senha inválidos."
                };
            }

            return new AuthResponseDto
            {
                Success = true,
                Message = "Login realizado com sucesso.",
                UserId = user.Id,
                Email = user.Email
            };
        }

        public async Task<AuthResponseDto> LogoutAsync()
        {
            await _signInManager.SignOutAsync();
            return new AuthResponseDto
            {
                Success = true,
                Message = "Logout realizado com sucesso."
            };
        }
    }
}
