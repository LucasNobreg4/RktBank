using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RktBank.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Registra um novo usuário no sistema
        /// </summary>
        /// <param name="registerDto">Dados de registro do usuário</param>
        /// <returns>Resultado do registro</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Dados inválidos fornecidos."
                });
            }

            try
            {
                var result = await _authService.RegisterAsync(registerDto);

                if (!result.Success)
                {
                    _logger.LogWarning("Falha no registro do usuário: {Email}", registerDto.Email);
                    return BadRequest(result);
                }

                _logger.LogInformation("Usuário registrado com sucesso: {UserId}", result.UserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar usuário: {Email}", registerDto.Email);
                return StatusCode(500, new AuthResponseDto
                {
                    Success = false,
                    Message = "Ocorreu um erro ao processar sua solicitação."
                });
            }
        }

        /// <summary>
        /// Realiza login de um usuário
        /// </summary>
        /// <param name="loginDto">Credenciais de login</param>
        /// <returns>Resultado do login</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponseDto
                {
                    Success = false,
                    Message = "Email e senha são obrigatórios."
                });
            }

            try
            {
                var result = await _authService.LoginAsync(loginDto);

                if (!result.Success)
                {
                    _logger.LogWarning("Tentativa de login falhou para: {Email}", loginDto.Email);
                    return Unauthorized(result);
                }

                _logger.LogInformation("Login bem-sucedido para usuário: {UserId}", result.UserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao realizar login: {Email}", loginDto.Email);
                return StatusCode(500, new AuthResponseDto
                {
                    Success = false,
                    Message = "Ocorreu um erro ao processar sua solicitação."
                });
            }
        }

        /// <summary>
        /// Realiza logout do usuário atual
        /// </summary>
        /// <returns>Resultado do logout</returns>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var result = await _authService.LogoutAsync();
                _logger.LogInformation("Usuário realizou logout com sucesso");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao realizar logout");
                return StatusCode(500, new AuthResponseDto
                {
                    Success = false,
                    Message = "Ocorreu um erro ao processar sua solicitação."
                });
            }
        }

        /// <summary>
        /// Verifica se o usuário está autenticado
        /// </summary>
        /// <returns>Status de autenticação</returns>
        [HttpGet("check")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult CheckAuth()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            return Ok(new
            {
                authenticated = true,
                userId = userId,
                email = email,
                userName = User.Identity?.Name
            });
        }
    }
}
