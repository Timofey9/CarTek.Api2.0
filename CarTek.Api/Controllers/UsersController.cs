using AutoMapper;
using CarTek.Api.Const;
using CarTek.Api.Exceptions;
using CarTek.Api.Model;
using CarTek.Api.Model.Dto;
using CarTek.Api.Model.Response;
using CarTek.Api.Services;
using CarTek.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace CarTek.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = AuthPolicies.ADMIN_ONLY)]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        public UserController(ILogger<UserController> logger, IUserService userService, IMapper mapper)
        {
            _mapper = mapper;
            _logger = logger;
            _userService = userService;
        }


        [HttpPost("registeruser")]
        public async Task<IActionResult> RegisterUser([FromBody]CreateUserModel user)
        {
            try
            {
                var result = await _userService.RegisterUser(user);

                if (!result.IsSuccess)
                {
                    return BadRequest(result.Message);
                }

                return Ok(result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Не удалось создать пользователя:{user.Login}:{ex.Message}");
                return BadRequest($"Не удалось создать пользователя:{user.Login}:{ex.Message}");
            }
        }

        [HttpGet("{login}")]
        public IActionResult GetUser(string login)
        {
            var user = _userService.GetByLogin(login);

            if(user != null)
                return Ok(_mapper.Map<UserModel>(user));

            return NotFound();
        }


        [HttpGet("all")]
        public IActionResult GetUsers(string? sortColumn, string? sortDirection, int pageNumber, int pageSize, string? searchColumn, string? search)
        {
            var list = _userService.GetAll(sortColumn, sortDirection, pageNumber, pageSize, searchColumn, search);
            var totalNumber = _userService.GetAll(searchColumn, search).Count();

            return Ok(new PagedResult<UserModel>()
            {
                TotalNumber = totalNumber,
                List = _mapper.Map<List<UserModel>>(list)
            });
        }

        [HttpDelete("deleteuser/{login}")]
        public async Task<IActionResult> DeleteUser(string login)
        {
            var user = await _userService.DeleteUser(login);

            if (user == null) {
                return NotFound(login);
            }

            return Ok(user);
        }

        [HttpPatch("updateuser/{login}")]
        public async Task<IActionResult> UpdateUser(string login, [FromBody] JsonPatchDocument<User> patchDoc)
        {
            var result = await _userService.UpdateUser(login, patchDoc);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Message);
        }
    }
}
