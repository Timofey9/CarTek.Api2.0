using CarTek.Api.Const;
using CarTek.Api.Exceptions;
using CarTek.Api.Model;
using CarTek.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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

        public AuthController(IUserService userService, ILogger<AuthController> logger)
        {
            _logger = logger;
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult UserAuth(UserAuthModel model)
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
                _logger.LogWarning(e.Message);
                return Unauthorized("Неверное имя пользователя");
            }
            catch (InvalidPasswordException e)
            {
                _logger.LogWarning(e.Message);
                return Unauthorized("Неверный пароль");
            }
        }
    }
}
