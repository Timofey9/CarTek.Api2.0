using AutoMapper;
using CarTek.Api.Const;
using CarTek.Api.Model;
using CarTek.Api.Model.Dto;
using CarTek.Api.Model.Orders;
using CarTek.Api.Model.Response;
using CarTek.Api.Services;
using CarTek.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarTek.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = AuthPolicies.ADMIN_ONLY)]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;
        private readonly IReportGeneratorService _reportGeneratorService;

        public OrderController(IOrderService orderService, IMapper mapper, IReportGeneratorService reportGeneratorService)
        {
            _orderService = orderService;
            _mapper = mapper;
            _reportGeneratorService = reportGeneratorService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderModel driver)
        {
            var res = await _orderService.CreateOrder(driver);
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

        [HttpGet("getordersbetweendates")]
        public IActionResult GetOrders(string? sortColumn, string? sortDirection, int pageNumber, int pageSize, string? searchColumn, string? search, DateTime startDate, DateTime endDate)
        {
            var list = _orderService.GetAll(sortColumn, sortDirection, pageNumber, pageSize, searchColumn, search, startDate, endDate);
            var totalNumber = _orderService.GetAll(searchColumn, search).Count();

            return Ok(new PagedResult<OrderModel>()
            {
                TotalNumber = totalNumber,
                List = _mapper.Map<List<OrderModel>>(list)
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
    }
}
