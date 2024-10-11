using CarTek.Api.Exceptions;
using CarTek.Api.Model;
using CarTek.Api.Services;
using CarTek.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarTek.Api.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;
        private readonly INotificationService _notificationService;
        private readonly IJwtService _jwtService;
        public AuthController(IUserService userService, ILogger<AuthController> logger, INotificationService notificationService, IJwtService jwtService)
        {
            _logger = logger;
            _userService = userService;
            _notificationService = notificationService;
            _jwtService = jwtService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> UserAuth(UserAuthModel model)
        {
            try
            {
                _logger.LogInformation($"User {model.Login} auth attempt");
                var userAuthResult = _userService.Authenticate(model);
                _logger.LogInformation($"User {model.Login} successfully authenticated");

                return Ok(userAuthResult);
            }
            catch (InvalidUsernameException e)
            {
                _logger.LogWarning(e, $"Неверное имя пользователя {model.Login}");
                return Unauthorized("Неверное имя пользователя");
            }
            catch (InvalidPasswordException e)
            {
                _logger.LogWarning(e, $"Неверный пароль {model.Password}");
                return Unauthorized("Неверный пароль");
            }
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult>Refresh([FromBody]TokenApiModel tokenApiModel)
        {
            if (tokenApiModel is null)
                return BadRequest("Invalid client request");

            var res = await _jwtService.Refresh(tokenApiModel);

            if (res == null)
                return BadRequest("Неверные данные для входа");

            return Ok(res);
        }
    }
}
