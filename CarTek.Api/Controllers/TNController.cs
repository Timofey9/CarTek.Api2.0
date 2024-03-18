using CarTek.Api.Model;
using CarTek.Api.Model.Response;
using CarTek.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarTek.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TNController : ControllerBase
    {
        private readonly ITnService _tnService;

        public TNController(ITnService tnService)
        {
            _tnService = tnService;
        }

        [HttpGet("gettnslist")]
        public IActionResult GetTNsList(string? sortColumn, string? sortDirection, int pageNumber, int pageSize, string? searchColumn, string? search, DateTime startDate, DateTime endDate)
        {
            try
            {
                var list = _tnService.GetAllPagination(sortColumn, sortDirection, pageNumber, pageSize, searchColumn, search, startDate, endDate);

                var totalNumber = _tnService.GetAll(searchColumn, search, startDate, endDate).Count();

                return Ok(new PagedResult<TNModel>()
                {
                    TotalNumber = totalNumber,
                    List = list.ToList()
                });
            }
            catch(Exception ex)
            {                                
                return BadRequest(ex.Message);            
            }
        }

    }
}
