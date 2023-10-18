using AutoMapper;
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

namespace CarTek.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DriversController : ControllerBase
    {
        private readonly ILogger<DriversController> _logger;
        private readonly IDriverService _driverService;
        private readonly IDriverTaskService _driverTaskService;
        private readonly IMapper _mapper;

        public DriversController(ILogger<DriversController> logger, IDriverService driverService, IMapper mapper, IDriverTaskService driverTaskService)
        {
            _logger = logger;
            _driverService = driverService;
            _mapper = mapper;
            _driverTaskService = driverTaskService;
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

            return Ok(_mapper.Map<List<DriverModel>>(list));
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

            var res = _mapper.Map<DriverTaskExportModel>(task);

            if(task != null)
            {
                _driverTaskService.DriverTaskExportModelSetLocations(res);
            }

            return Ok(res);
        }


        [HttpPost("taskgetback")]
        public IActionResult TaskGetBack([FromBody] DriverTaskUpdateModel model)
        {
            var res = _driverTaskService.TaskGetBack(model.DriverTaskId);

            if (res.IsSuccess)
            {
                return Ok();
            }

            return BadRequest(res);
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
                return BadRequest(result);
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

        [HttpPost("starttn")]
        public async Task <IActionResult> StartTn([FromForm] FillDocumentModel model)
        {
            var res = _driverTaskService.StartDocument(model);

            if (model.IsSubtask)
            {
                await _driverTaskService.UpdateDriverSubTask(model.DriverTaskId, model.Files, model.UpdatedStatus, model.Note);
            }
            else
            {
                await _driverTaskService.UpdateDriverTask(model.DriverTaskId, model.Files, model.UpdatedStatus, model.Note);
            }

            if (res.IsSuccess)
            {
                return Ok(res);
            }

            return BadRequest(res);
        
        }

        [HttpPost("finalizetn")]
        public async Task<IActionResult> FinalizeTn([FromForm] FillDocumentModel model)
        {
            var res = _driverTaskService.FinalizeDocument(model);

            if (model.IsSubtask)
            {
                await _driverTaskService.UpdateDriverSubTask(model.DriverTaskId, model.Files, model.UpdatedStatus, model.Note);
            }
            else
            {
                await _driverTaskService.UpdateDriverTask(model.DriverTaskId, model.Files, model.UpdatedStatus, model.Note);
            }

            if (res.IsSuccess)
            {
                return Ok(res);
            }

            return BadRequest(res);
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
                return BadRequest(result);
            }
        }
    }
}
