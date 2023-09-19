using AutoMapper;
using CarTek.Api.Model;
using CarTek.Api.Model.Orders;
using CarTek.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarTek.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UtilsController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IClientService _clientService;
        private readonly IAddressService _addressService;
        private readonly IMapper _mapper;
        private readonly IReportGeneratorService _reportGeneratorService;
        private readonly IDriverTaskService _driverTaskService;

        public UtilsController(IOrderService orderService, IClientService clientService,
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

        [HttpGet("getmaterials")]
        public IActionResult GetMaterials()
        {
            var list = _orderService.GetMaterials();

            return Ok(list);
        }


        [HttpGet("getaddresses")]
        public IActionResult GetAddresses()
        {
            var list = _addressService.GetAddresses();

            return Ok(list);
        }

        [HttpGet("getclients")]
        public IActionResult GetClients()
        {
            var list = _clientService.GetClients();

            return Ok(list);
        }

        [HttpPost("createclient")]
        public IActionResult CreateClient([FromBody] CreateClientModel model)
        {
            var result = _clientService.CreateClient(model.ClientName, model.Inn, model.ClientAddress);

            return Ok(result);
        }

        [HttpPost("createaddress")]
        public IActionResult CreateAddress([FromBody] CrateAddressModel model)
        {
            var result = _addressService.CreateAddress(model.Coordinates, model.TextAddress);

            return Ok(result);
        }

        [HttpPost("creatematerial")]
        public IActionResult CreateMaterial([FromBody] CreateMaterialModel model)
        {
            var res = _orderService.AddMaterial(model.Name);

            return Ok(res);
        }
    }
}
