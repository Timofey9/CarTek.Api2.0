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
using Microsoft.DotNet.Scaffolding.Shared.CodeModifier.CodeChange;
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
        private readonly IMapper _mapper;
        private readonly IReportGeneratorService _reportGeneratorService;
        private readonly IDriverTaskService _driverTaskService;

        public OrderController(IOrderService orderService, IClientService clientService,
            IAddressService addresSservice, IMapper mapper, IReportGeneratorService reportGeneratorService,
            IDriverTaskService driverTaskService)
        {
            _orderService = orderService;
            _addressService = addresSservice;
            _clientService = clientService;
            _mapper = mapper;
            _reportGeneratorService = reportGeneratorService;
            _driverTaskService = driverTaskService;
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
            var totalNumber = _orderService.GetAllBetweenDates(startDate, endDate).Count();

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

            var result = _mapper.Map<List<OrderModel>>(list);

            return Ok(result);
        }

        [HttpGet("getmaterials")]
        public IActionResult GetMaterials()
        {
            var list = _orderService.GetMaterials();

            return Ok(list);
        }

        [HttpGet("getxls")]
        public IActionResult TestFileDownload(DateTime startDate, DateTime endDate)
        {
            try
            {
                var orders = _orderService.GetAllBetweenDates(startDate, endDate);

                var fileStream = _reportGeneratorService.GenerateOrdersReport(orders);

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

        [HttpPost("updatedrivertask")]
        public async Task<IActionResult> UpdateDriverTask([FromBody] AdminUpdateTaskModel model)
        {
            var result = await _driverTaskService.AdminUpdateDriverTask(model.TaskId, model.CarId, model.DriverId, model.AdminComment);
            
            return Ok(result);
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

    }
}
