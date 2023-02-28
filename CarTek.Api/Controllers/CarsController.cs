using AutoMapper;
using CarTek.Api.Model;
using CarTek.Api.Model.Dto;
using CarTek.Api.Model.Response;
using CarTek.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarTek.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CarsController : ControllerBase
    {
        private readonly ILogger<CarsController> _logger;
        private readonly ICarService _carService;
        private readonly IMapper _mapper;

        public CarsController(ILogger<CarsController> logger, ICarService carService, IMapper mapper)
        {
            _logger = logger;
            _carService = carService;
            _mapper = mapper;
        }


        [HttpPost("createcar")]
        public IActionResult CreateCar([FromBody]CreateCarModel car)
        {
            var carEntity = _carService.CreateCar(car);
            if(carEntity == null)
            {
                return Conflict(car);
            }

            return Ok(carEntity);
        }

        [HttpGet("plate/{plate}")]
        public IActionResult GetCarByPlate(string plate)
        {
            var car = _carService.GetByPlate(plate);

            if (car == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<CarModel>(car));
        }


        [HttpGet("car/{id}")]
        public IActionResult GetCar(long carId)
        {
            var car = _carService.GetById(carId);

            if(car == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<CarModel>(car));
        }

        [HttpDelete("deletecar/{carId}")]
        public IActionResult DeleteCar(long carId)
        {
            var res = _carService.DeleteCar(carId);
            if(res != null)
            {
                return Ok();
            }
            return BadRequest();
        }


        [HttpGet("getcars")]
        public IActionResult GetCars(string? sortColumn, string? sortDirection, int pageNumber, int pageSize, string? searchColumn, string? search)
        {
            var list = _carService.GetAll(sortColumn, sortDirection, pageNumber, pageSize, searchColumn, search);
            var totalNumber = _carService.GetAll(searchColumn, search).Count();

            return Ok(new PagedResult<CarModel>()
            {
                TotalNumber = totalNumber,
                List = _mapper.Map<List<CarModel>>(list)
            });
        }

        [HttpGet("getallcars")]
        public IActionResult GetAllCars()
        {
            var list = _carService.GetAll(null, null, 0, 0, null, null);

            return Ok(_mapper.Map<List<CarModel>>(list));            
        }
    }
}
