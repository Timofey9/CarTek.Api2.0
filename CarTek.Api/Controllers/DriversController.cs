using AutoMapper;
using CarTek.Api.Const;
using CarTek.Api.Model;
using CarTek.Api.Model.Dto;
using CarTek.Api.Model.Orders;
using CarTek.Api.Model.Response;
using CarTek.Api.Services;
using CarTek.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;

namespace CarTek.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DriversController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<DriversController> _logger;
        private readonly IDriverService _driverService;
        private readonly IDriverTaskService _driverTaskService;
        private readonly IMapper _mapper;

        public DriversController(ILogger<DriversController> logger, IDriverService driverService, 
            IMapper mapper, IDriverTaskService driverTaskService, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _driverService = driverService;
            _mapper = mapper;
            _driverTaskService = driverTaskService;
            _httpContextAccessor = httpContextAccessor;
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

            return Ok(_mapper.Map<DriverModel>(user));
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

            return Ok(_mapper.Map<List<DriverModel>>(list));
        }

        [HttpGet("getalldriverswithfired")]
        public IActionResult GetAllDriversWithFired(string? sortColumn, string? sortDirection, int pageNumber, int pageSize, string? searchColumn, string? search)
        {
            var list = _driverService.GetAll(sortColumn, sortDirection, pageNumber, pageSize, searchColumn, search, true);
            var totalNumber = _driverService.GetAll(searchColumn, search, true).Count();

            return Ok(new PagedResult<DriverModel>()
            {
                TotalNumber = totalNumber,
                List = _mapper.Map<List<DriverModel>>(list)
            });
        }

        [HttpGet("getdrivertasks")]
        public IActionResult GetDriverTasks(int pageNumber, int pageSize, DateTime? startDate, DateTime? endDate, long driverId, string? searchBy, string? searchString)
        {
            var list = _driverTaskService.GetDriverTasksFiltered(pageNumber, pageSize, startDate, endDate, driverId, searchBy, searchString);

            var listRes = _driverTaskService.MapAndExtractLocationsInfo(list);

            var totalNumber = _driverTaskService.GetDriverTasksAll(startDate, endDate, driverId, searchBy, searchString).Count();

            return Ok(new PagedResult<DriverTaskOrderModel>()
            {
                TotalNumber = totalNumber,
                List = listRes
            });
        }

        [HttpGet("getdrivertask/{driverTaskId}")]
        public IActionResult GetDriverTasks(long driverTaskId)
        {
            var task = _driverTaskService.GetDriverTaskById(driverTaskId);

            var user = _httpContextAccessor.HttpContext.User;
            if (user != null)
            {
                var id = user.Claims.SingleOrDefault(t => t.Type == AuthConstants.ClaimTypeId);
                var isAdmin = user.Claims.SingleOrDefault(t => t.Type == AuthConstants.ClaimTypeIsAdmin);
                var isDriver = user.Claims.SingleOrDefault(t => t.Type == AuthConstants.ClaimTypeIsDriver);
                if (isAdmin == null && id != null)
                {
                    double.TryParse(id.Value, out var dId);

                    if (dId != task.DriverId)
                    {
                        return BadRequest("Нет прав!");
                    }
                }
            }

            var res = _mapper.Map<DriverTaskExportModel>(task);

            if(task != null)
            {
                _driverTaskService.DriverTaskExportModelSetLocations(res);
            }

            return Ok(res);
        }

        [HttpGet("getsubtask/{subtaskId}")]
        public IActionResult GetSubTask(long subtaskId)
        {
            var task = _driverTaskService.GetSubTask(subtaskId);

            var user = _httpContextAccessor.HttpContext.User;
            if (user != null)
            {
                var id = user.Claims.SingleOrDefault(t => t.Type == AuthConstants.ClaimTypeId);
                var isAdmin = user.Claims.SingleOrDefault(t => t.Type == AuthConstants.ClaimTypeIsAdmin);
                var isDriver = user.Claims.SingleOrDefault(t => t.Type == AuthConstants.ClaimTypeIsDriver);
                if (isAdmin == null && id != null)
                {
                    double.TryParse(id.Value, out var dId);

                    if (dId != task.DriverTask.Driver.Id)
                    {
                        return BadRequest("Нет прав!");
                    }
                }
            }

            return Ok(task);
        }

        [HttpPost("taskgetback")]
        public IActionResult TaskGetBack([FromBody] DriverTaskUpdateModel model)
        {
            var res = _driverTaskService.TaskGetBack(model.DriverTaskId, model.IsSubTask ?? false);

            if (res.IsSuccess)
            {
                return Ok();
            }

            return BadRequest(res.Message);
        }
               
        
        [HttpPost("firedriver")]
        public IActionResult FireDriver([FromBody] FireDriverModel model)
        {
            var res = _driverService.FireDriver(model.DriverId);

            if (res.IsSuccess)
            {
                return Ok();
            }

            return BadRequest(res.Message);
        }

        [HttpPost("updateDriverTask")]
        public async Task<IActionResult> CreateDriverTasks([FromForm] UpdateDriverTaskModel driverTaskModel)
        {
            var result = await _driverTaskService.UpdateDriverTask(driverTaskModel.DriverTaskId, 
                driverTaskModel.Files,
                driverTaskModel.UpdatedStatus,
                driverTaskModel.Note);

            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result.Message);
            }
        }

        [HttpPost("postnote")]
        public async Task<IActionResult> PostDriverTaskNote([FromForm] PostNoteModel driverTaskModel)
        {
            var result = await _driverTaskService.SubmitDtNote(driverTaskModel.DriverTaskId,
                driverTaskModel.Files,
                driverTaskModel.Note);

            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        [HttpPost("subtasktn")]
        public async Task<IActionResult> CreateSubtaskTn([FromForm] FillDocumentModel model)
        {
            var res = _driverTaskService.CreateSubtaskTn(model);
            await _driverTaskService.UpdateDriverSubTask(model.SubTaskId.Value, model.Files, model.UpdatedStatus, model.Note);

            if (res.IsSuccess)
            {
                return Ok(res);
            }

            return BadRequest(res.Message);
        }

        [HttpPost("starttn")]
        public async Task <IActionResult> StartTn([FromForm] FillDocumentModel model)
        {
            var res = _driverTaskService.StartDocument(model);

            if (model.IsSubtask)
            {
                await _driverTaskService.UpdateDriverSubTask(model.SubTaskId.Value, model.Files, model.UpdatedStatus, model.Note);
            }
            else
            {
                await _driverTaskService.UpdateDriverTask(model.DriverTaskId, model.Files, model.UpdatedStatus, model.Note);
            }

            if (res.IsSuccess)
            {
                return Ok(res);
            }

            return BadRequest(res.Message);
        
        }

        [HttpPost("finalizetn")]
        public async Task<IActionResult> FinalizeTn([FromForm] FillDocumentModel model)
        {
            var res = _driverTaskService.FinalizeDocument(model);

            if (model.IsSubtask)
            {
                await _driverTaskService.UpdateDriverSubTask(model.SubTaskId.Value, model.Files, model.UpdatedStatus, model.Note);
            }
            else
            {
                await _driverTaskService.UpdateDriverTask(model.DriverTaskId, model.Files, model.UpdatedStatus, model.Note);
            }

            if (res.IsSuccess)
            {
                return Ok(res);
            }

            return BadRequest(res.Message);
        }
                
        
        [HttpPost("updatetn")]
        public async Task<IActionResult> UpdateTn([FromForm] FillDocumentModel model)
        {
            var res = await _driverTaskService.UpdateTN(model);

            if (res.IsSuccess)
            {
                return Ok(res);
            }

            return BadRequest(res.Message);
        }

        [HttpPost("updatesubtask")]
        public async Task<IActionResult> UpdateSubTask([FromForm] UpdateDriverTaskModel driverTaskModel)
        {
            var result = await _driverTaskService.UpdateDriverSubTask(driverTaskModel.DriverTaskId,
                driverTaskModel.Files,
                driverTaskModel.UpdatedStatus,
                driverTaskModel.Note);

            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result.Message);
            }
        }
    }
}
