using AutoMapper;
using CarTek.Api.Model;
using CarTek.Api.Model.Dto;
using CarTek.Api.Model.Dto.Car;
using CarTek.Api.Model.Orders;
using CarTek.Api.Model.Response;
using CarTek.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.Formula.Functions;
using System.Collections.Generic;

namespace CarTek.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IClientService _clientService;
        private readonly IAddressService _addressService;
        private readonly ICarService _carService;
        private readonly IMapper _mapper;
        private readonly IReportGeneratorService _reportGeneratorService;
        private readonly IDriverTaskService _driverTaskService;

        public OrderController(IOrderService orderService, IClientService clientService,
            IAddressService addresSservice, IMapper mapper, IReportGeneratorService reportGeneratorService,
            IDriverTaskService driverTaskService, ICarService carService)
        {
            _orderService = orderService;
            _addressService = addresSservice;
            _clientService = clientService;
            _mapper = mapper;
            _reportGeneratorService = reportGeneratorService;
            _driverTaskService = driverTaskService;
            _carService = carService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderModel order)
        {
            var res = await _orderService.CreateOrder(order);
            if (res.IsSuccess)
            {
                return Ok(res);
            }
            return BadRequest(res.Message);
        }


        [HttpPost("createtask")]
        public async Task<IActionResult> CreateTask([FromBody] CreateDriverTaskModel task)
        {
            var res = await _orderService.CreateDriverTask(task);
            if (res.IsSuccess)
            {
                return Ok(res);
            }
            return BadRequest(res);
        }

        [HttpPost("createtasksmultiple")]
        public async Task<IActionResult> CreateTasks([FromBody] CreateDriverMultipleTaskModel tasks)
        {
            var result = new List<ApiResponse>();

            foreach(var task in tasks.Tasks)
            {
                var res = await _orderService.CreateDriverTask(task);
                result.Add(res);
            }

            var successCount = result.Where(t => t.IsSuccess).Count();
                
            return Ok(new ApiResponse            
            {            
                IsSuccess = true,            
                Message = $"Создано {successCount} задач из {tasks.Tasks.Count}"                
            });
        }

        [HttpGet("getordersbetweendates")]
        public IActionResult GetOrders(string? sortColumn, string? sortDirection, int pageNumber, int pageSize, string? searchColumn, string? search, DateTime startDate, DateTime endDate)
        {
            var list = _orderService.GetAll(sortColumn, sortDirection, pageNumber, pageSize, searchColumn, search, startDate, endDate);
           
            var totalNumber = _orderService.GetOrderModelsBetweenDates(searchColumn, search, startDate, endDate).Count();

            var mappedList = new List<OrderModel>();

            foreach (var item in list)
            {
                var gp = _clientService.GetClient(item.GpId);

                var mappedItem = _mapper.Map<OrderModel>(item);

                mappedItem.Gp = _mapper.Map<ClientModel>(gp);

                mappedList.Add(mappedItem);
            }

            return Ok(new PagedResult<OrderModel>()
            {
                TotalNumber = totalNumber,
                List = mappedList
            });
        }

        [HttpGet("getallactiveorders")]
        public IActionResult GetAllActiveOrders(DateTime startDate)
        {
            var list = _orderService.GetAllActive(startDate);
            var mappedList = new List<OrderModel>();

            foreach (var item in list)
            {
                var gp = _clientService.GetClient(item.GpId);

                var mappedItem = _mapper.Map<OrderModel>(item);

                mappedItem.Gp = _mapper.Map<ClientModel>(gp);

                mappedList.Add(mappedItem);
            }

            return Ok(mappedList);
        }


        [HttpGet("gettasksreport")]
        public IActionResult DownloadTasksList(DateTime startDate)
        {
            try
            {
                var tasks = _carService.GetCarsWithTasks(startDate);

                var fileStream = _reportGeneratorService.GenerateTasksReport(startDate, tasks);

                var contentType = "application/octet-stream";

                var result = new FileContentResult(fileStream.ToArray(), contentType);

                return result;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }               
        
        [HttpGet("getfulltasksreport")]
        public IActionResult DownloadFullTasksList(DateTime startDate, DateTime endDate)
        {
            try
            {
                var tasks = _driverTaskService.GetDriverTasksBetweenDates(startDate, endDate);

                var fileStream = _reportGeneratorService.GenerateTasksReportFull(startDate, endDate, tasks);

                var contentType = "application/octet-stream";

                var result = new FileContentResult(fileStream.ToArray(), contentType);

                return result;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }        
        
        [HttpGet("gettasksreportshort")]
        public IActionResult DownloadTasksListShort(DateTime startDate)
        {
            try
            {
                var tasks = _carService.GetCarsWithTasks(startDate);

                var fileStream = _reportGeneratorService.GenerateTasksReportShort(startDate, tasks);

                var contentType = "application/octet-stream";

                var result = new FileContentResult(fileStream.ToArray(), contentType);

                return result;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("gettnxls")]
        public IActionResult DownloadTNsList(DateTime startDate, DateTime endDate)
        {
            try
            {
                var orders = _orderService.GetTNsBetweenDates(startDate, endDate);

                var fileStream = _reportGeneratorService.GenerateTnsReport(orders, startDate, endDate);

                var contentType = "application/octet-stream";

                var result = new FileContentResult(fileStream.ToArray(), contentType);

                return result;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }        
        
        [HttpGet("getaccountantreport")]
        public IActionResult DownloadAccountantReportList(DateTime startDate, DateTime endDate)
        {
            try
            {
                var orders = _orderService.GetTNsBetweenDates(startDate, endDate, true);

                var fileStream = _reportGeneratorService.GenerateSalariesReport(orders, startDate, endDate);

                var contentType = "application/octet-stream";

                var result = new FileContentResult(fileStream.ToArray(), contentType);

                return result;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("getdriverreport")]
        public IActionResult DownloadDriverReportList(DateTime startDate, DateTime endDate, long driverId)
        {
            try
            {
                var tns = _orderService.GetTNsBetweenDatesDriver(startDate, endDate,driverId, true);

                var fileStream = _reportGeneratorService.GenerateSalaryReportDriver(tns, startDate, endDate);

                var contentType = "application/octet-stream";

                var result = new FileContentResult(fileStream.ToArray(), contentType);

                return result;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("getxls")]
        public IActionResult TestFileDownload(DateTime startDate, DateTime endDate)
        {
            try
            {
                var orders = _orderService.GetOrderModelsBetweenDates(null, null, startDate, endDate);

                var sorted = orders.OrderBy(t => t.StartDate);

                var mappedList = new List<OrderModel>();

                foreach (var item in sorted)
                {
                    var gp = _clientService.GetClient(item.GpId);

                    var mappedItem = _mapper.Map<OrderModel>(item);

                    mappedItem.Gp = _mapper.Map<ClientModel>(gp);

                    mappedList.Add(mappedItem);
                }

                var fileStream = _reportGeneratorService.GenerateOrdersReport(mappedList, startDate, endDate);

                var contentType = "application/octet-stream";

                var result = new FileContentResult(fileStream.ToArray(), contentType);

                return result;
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("gettn")]
        public IActionResult DownloadTN(long driverTaskId)
        {
            try
            {
                var tnModel = _driverTaskService.GetTnModel(driverTaskId);

                var fileStream = _reportGeneratorService.GenerateTn(tnModel);

                var contentType = "application/octet-stream";

                var result = new FileContentResult(fileStream.ToArray(), contentType);

                return result;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("viewtn/{driverTaskId}/{isSubTask}")]
        public IActionResult ViewTN(long driverTaskId, string isSubTask)
        {
            try
            {
                TNModel tnModel;

                if (!string.IsNullOrEmpty(isSubTask) && isSubTask.Equals("true"))
                {
                    tnModel = _driverTaskService.GetTnModel(driverTaskId, true);
                }
                else
                {
                    tnModel = _driverTaskService.GetTnModel(driverTaskId);
                }

                return Ok(tnModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("viewedittn/{driverTaskId}/{isSubTask}")]
        public IActionResult ViewEditTN(long driverTaskId, string isSubTask)
        {
            try
            {
                EditTNModel tnModel;

                if (!string.IsNullOrEmpty(isSubTask) && isSubTask.Equals("true"))
                {
                    tnModel = _driverTaskService.GetEditTnModel(driverTaskId, true);
                }
                else
                {
                    tnModel = _driverTaskService.GetEditTnModel(driverTaskId);
                }

                return Ok(tnModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("updatedrivertask")]
        public async Task<IActionResult> UpdateDriverTask([FromBody] AdminUpdateTaskModel model)
        {
            var result = await _driverTaskService.AdminUpdateDriverTask(model.TaskId, model.CarId, model.DriverId, model.AdminComment, model.StartDate, model.Shift, model.OrderId);
            
            return Ok(result);
        }


        [HttpPost("canceldrivertask")]
        public IActionResult CancelDriverTask([FromBody] CancelTaskRequest model)
        {
            var result = _driverTaskService.CancelDriverTask(model.DriverTaskId);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }      
        
        [HttpPost("restoredrivertask")]
        public IActionResult RestoreDriverTask([FromBody] CancelTaskRequest model)
        {
            var result = _driverTaskService.RestoreDriverTask(model.DriverTaskId);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPost("canceldriversubtask")]
        public IActionResult CancelDriverSubTask([FromBody] CancelTaskRequest model)
        {
            var result = _driverTaskService.CancelDriverSubTask(model.DriverTaskId);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPost("restoredriversubtask")]
        public IActionResult RestoreDriverSubTask([FromBody] CancelTaskRequest model)
        {
            var result = _driverTaskService.RestoreDriverSubTask(model.DriverTaskId);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPost("deleteS3Image")]
        public async Task<IActionResult> DeleteS3Image([FromBody] DeleteImageRequest model)
        {
            var result = await _driverTaskService.DeleteImage(model);

            if(result.IsSuccess) {
                return Ok(result);
            }

            return BadRequest();
        }


        [HttpDelete("deleteSubTask/{subtaskId}")]
        public IActionResult DeleteSubtask(long subtaskId)
        {
            var result = _driverTaskService.DeleteSubTask(subtaskId);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest();
        }


        [HttpGet("getorderbyid/{orderId}")]
        public IActionResult GetOrderById(long orderId)
        {
            var result = _orderService.GetOrderExportById(orderId);
            if(result != null)
            {
                return Ok(result);
            }
            return (BadRequest(new ApiResponse
            {
                IsSuccess = false,
                Message = "Не удалось получить заявку"
            }));
        }

        [HttpDelete("deleteorder/{orderId}")]
        public IActionResult DeleteOrder(long orderId)
        {
            var res = _orderService.DeleteOrder(orderId);
            if (res != null)
            {
                return Ok(res);
            }
            return BadRequest(res);
        }

        [HttpDelete("deletetask/{taskId}")]
        public IActionResult DeleteTask(long taskId)
        {
            var res = _driverTaskService.DeleteDriverTask(taskId);
            if (res != null)
            {
                return Ok(res);
            }
            return BadRequest(res);
        }

        [HttpPatch("updateorder/{id}")]
        public IActionResult UpdateDriver(long id, [FromBody] JsonPatchDocument<Order> patchDoc)
        {
            var res = _orderService.UpdateOrder(id, patchDoc);

            if (!res.IsSuccess)
            {
                return BadRequest(res);
            }

            return Ok(res);
        }

        [HttpPost("verifytn")]
        public IActionResult VerifyTn([FromBody]VerifyTnModel model)
        {
            var res = _driverTaskService.VerifyTn(model.DriverTaskId, model.IsOriginalReceived, model.IsSubtask ?? false);

            if (!res.IsSuccess)
            {
                return BadRequest(res);
            }
            return Ok();
        }

        [HttpPost("createsubtask")]
        public IActionResult CreateSubTask([FromBody]CreateSubTaskModel model)
        {
            var res = _driverTaskService.CreateSubTask(model.DriverTaskId);

            if (!res.IsSuccess)
            {
                return BadRequest(res);
            }

            return Ok(res);
        }
    }
}
