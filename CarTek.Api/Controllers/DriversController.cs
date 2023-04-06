using AutoMapper;
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
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DriversController : ControllerBase
    {
        private readonly ILogger<DriversController> _logger;
        private readonly IDriverService _driverService;
        private readonly IMapper _mapper;

        public DriversController(ILogger<DriversController> logger, IDriverService driverService, IMapper mapper)
        {
            _logger = logger;
            _driverService = driverService;
            _mapper = mapper;
        }

        [HttpPost("createdriver")]
        public IActionResult CreateDriver([FromBody] CreateDriverModel driver)
        {
            var res = _driverService.CreateDriver(driver);
            if (res.IsSuccess)
            {
                return Ok(res.Message);
            }
            return BadRequest(res.Message);
        }

        [HttpPatch("updatedriver/{id}")]
        public IActionResult UpdateDriver(long id, [FromBody] JsonPatchDocument<Driver> patchDoc)
        {
            var user = _driverService.UpdateDriver(id, patchDoc);

            if (user == null)
            {
                return BadRequest();
            }

            return Ok(user);
        }


        [HttpGet("driver/{id}")]
        public IActionResult GetDriver(long id)
        {
            var driver = _driverService.GetById(id);

            if (driver == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<DriverModel>(driver));
        }

        [HttpDelete("deletedriver/{driverId}")]
        public IActionResult DeleteDriver(long driverId)
        {
            var res = _driverService.DeleteDriver(driverId);
            if (res != null)
            {
                return Ok();
            }
            return BadRequest();
        }


        [HttpGet("getdrivers")]
        public IActionResult GetDrivers(string? sortColumn, string? sortDirection, int pageNumber, int pageSize, string? searchColumn, string? search)
        {
            var list = _driverService.GetAll(sortColumn, sortDirection, pageNumber, pageSize, searchColumn, search);
            var totalNumber = _driverService.GetAll(searchColumn, search).Count();

            return Ok(new PagedResult<DriverModel>()
            {
                TotalNumber = totalNumber,
                List = _mapper.Map<List<DriverModel>>(list)
            });
        }

        [HttpGet("getalldrivers")]
        public IActionResult GetAllDrivers()
        {
            var list = _driverService.GetAll();

            return Ok(list);
        }
    }
}
