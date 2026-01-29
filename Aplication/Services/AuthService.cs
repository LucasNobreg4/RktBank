using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Interfaces;
using Domain.Entities;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;

        public AuthService(IUserRepository userRepository, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            var existingUser = await _userRepository.FindByEmailAsync(registerDto.Email);
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

            var (succeeded, errors) = await _userRepository.CreateUserAsync(user, registerDto.Password);

            if (!succeeded)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = string.Join(", ", errors)
                };
            }

            var token = _tokenService.GenerateToken(user);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Usuário registrado com sucesso.",
                UserId = user.Id,
                Email = user.Email,
                Token = token
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userRepository.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Email ou senha inválidos."
                };
            }

            var (succeeded, isLockedOut) = await _userRepository.ValidatePasswordAsync(user, loginDto.Password);

            if (!succeeded)
            {
                if (isLockedOut)
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

            var token = _tokenService.GenerateToken(user);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Login realizado com sucesso.",
                UserId = user.Id,
                Email = user.Email,
                Token = token
            };
        }

        public async Task<AuthResponseDto> LogoutAsync()
        {
            await _userRepository.SignOutAsync();
            return new AuthResponseDto
            {
                Success = true,
                Message = "Logout realizado com sucesso."
            };
        }
    }
}
