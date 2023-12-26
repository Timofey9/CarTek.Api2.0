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
        private readonly IMaterialService _materialService;
        private readonly IMapper _mapper;

        public UtilsController(IOrderService orderService, IClientService clientService,
            IAddressService addresSservice, IMapper mapper, IMaterialService materialService)
        {
            _orderService = orderService;
            _addressService = addresSservice;
            _clientService = clientService;
            _materialService = materialService;
            _mapper = mapper;
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
            var mappedList = _mapper.Map<List<ClientModel>>(list);
            return Ok(mappedList);
        }

        [HttpPost("createclient")]
        public IActionResult CreateClient([FromBody] CreateClientModel model)
        {
            var result = _clientService.CreateClient(model.ClientName, model.Inn, model.ClientAddress, model.ClientUnit, model.FixedPrice);

            return Ok(result);
        }

        [HttpPost("updateclient")]
        public IActionResult UpdateClient([FromBody] CreateClientModel model)
        {
            var result = _clientService.UpdateClient(model.Id, model.ClientName, model.Inn, model.ClientAddress, model.ClientUnit, model.FixedPrice);

            return Ok(result);
        }

        [HttpDelete("deleteclient/{clientId}")]
        public IActionResult DeleteClient(long clientId)
        {
            var result = _clientService.DeleteClient(clientId);

            return Ok(result);
        }

        [HttpPost("createaddress")]
        public IActionResult CreateAddress([FromBody] CrateAddressModel model)
        {
            var result = _addressService.CreateAddress(model.Coordinates, model.TextAddress);

            return Ok(result);
        }

        [HttpPost("updateaddress")]
        public IActionResult UpdateAddress([FromBody] CrateAddressModel model)
        {
            var result = _addressService.UpdateAddress(model.Id, model.Coordinates, model.TextAddress);

            return Ok(result);
        }

        [HttpDelete("deleteaddress/{addressId}")]
        public IActionResult DeleteAddress(long addressId)
        {
            var result = _addressService.DeleteAddress(addressId);

            return Ok(result);
        }

        [HttpPost("creatematerial")]
        public IActionResult CreateMaterial([FromBody] CreateMaterialModel model)
        {
            var res = _materialService.CreateMaterial(model.Name);

            return Ok(res);
        }

        [HttpPost("updatematerial")]
        public IActionResult UpdateMaterial([FromBody] CreateMaterialModel model)
        {
            var res = _materialService.UpdateMaterial(model.Id, model.Name);

            return Ok(res);
        }

        [HttpDelete("deletematerial/{materialId}")]
        public IActionResult DeleteMaterial(long materialId)
        {
            var result = _materialService.DeleteMaterial(materialId);

            return Ok(result);
        }
    }
}
