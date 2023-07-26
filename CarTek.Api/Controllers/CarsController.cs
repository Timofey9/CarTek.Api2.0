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
    public class CarsController : ControllerBase
    {
        private readonly ILogger<CarsController> _logger;
        private readonly ICarService _carService;
        private readonly ITrailerService _trailerService;
        private readonly IMapper _mapper;

        public CarsController(ILogger<CarsController> logger, ICarService carService, ITrailerService trailerService, IMapper mapper)
        {
            _logger = logger;
            _trailerService = trailerService;
            _carService = carService;
            _mapper = mapper;
        }


        [HttpPost("createtrailer")]
        public IActionResult CreateTrailer([FromBody] CreateTrailerModel trailer)
        {
            var trailerEntity = _trailerService.CreateTrailer(trailer);

            if (trailerEntity.IsSuccess)
            {
                return Ok(trailerEntity);
            }
            return Conflict(trailerEntity.Message);
        }


        [HttpPatch("updatetrailer/{id}")]
        public IActionResult UpdateTrailer(long id, [FromBody] JsonPatchDocument<Trailer> patchDoc)
        {
            var response = _trailerService.UpdateTrailer(id, patchDoc);

            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpPost("createcar")]
        public IActionResult CreateCar([FromBody]CreateCarModel car)
        {
            var carEntity = _carService.CreateCar(car);

            if(carEntity.IsSuccess)
            {
                return Ok(carEntity);
            }
            return Conflict(carEntity);
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

        [HttpGet("trailer/{plate}")]
        public IActionResult GetTrailerByPlate(string plate)
        {
            var trailer = _trailerService.GetByPlate(plate);

            if (trailer == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<TrailerModel>(trailer));
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
        
        [HttpGet("getcarswithtasks")]
        public IActionResult GetCarsWithTasks(DateTime startDate)
        {
            var result = _carService.GetCarsWithTasks(startDate);

            if(result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpDelete("deletetrailer/{trailerId}")]
        public IActionResult DeleteTrailer(long trailerId)
        {
            var res = _trailerService.DeleteTrailer(trailerId);
            if (res.IsSuccess)
            {
                return Ok(res.Message);
            }
            return BadRequest(res.Message);
        }

        [HttpDelete("deletecar/{carId}")]
        public IActionResult DeleteCar(long carId)
        {
            var res = _carService.DeleteCar(carId);
            if(res.IsSuccess)
            {
                return Ok(res.Message);
            }
            return BadRequest(res.Message);
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

        [HttpGet("gettrailers")]
        public IActionResult GetTrailers(string? sortColumn, string? sortDirection, int pageNumber, int pageSize, string? searchColumn, string? search)
        {
            var list = _trailerService.GetAll(sortColumn, sortDirection, pageNumber, pageSize, searchColumn, search);
            var totalNumber = _trailerService.GetAll(searchColumn, search).Count();

            return Ok(new PagedResult<TrailerModel>()
            {
                TotalNumber = totalNumber,
                List = _mapper.Map<List<TrailerModel>>(list)
            });
        }
        [HttpGet("getallcars")]
        public IActionResult GetAllCars()
        {
            var list = _carService.GetAll(null, null, 0, 0, null, null);

            return Ok(_mapper.Map<List<CarModel>>(list));            
        }

        [HttpGet("getalltrailers")]
        public IActionResult GetAllTrailers()
        {
            var list = _trailerService.GetAll(null, null, 0, 0, null, null);

            return Ok(_mapper.Map<List<TrailerModel>>(list));
        }

        [HttpPatch("updatecar/{id}")]
        public IActionResult UpdateDriver(long id, [FromBody] JsonPatchDocument<Car> patchDoc)
        {
            var car = _carService.UpdateCar(id, patchDoc);

            if (car == null)
            {
                return BadRequest();
            }

            return Ok(_mapper.Map<CarModel>(car));
        }
    }
}
