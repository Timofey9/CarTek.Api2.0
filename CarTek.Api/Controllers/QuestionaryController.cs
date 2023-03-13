using AutoMapper;
using CarTek.Api.Model;
using CarTek.Api.Model.Dto;
using CarTek.Api.Model.Quetionary;
using CarTek.Api.Model.Response;
using CarTek.Api.Services;
using CarTek.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;

namespace CarTek.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionaryController : ControllerBase 
    {
        private readonly IQuestionaryService _questionaryService;
        private readonly IMapper _mapper;

        public QuestionaryController(IQuestionaryService questionaryService, IMapper mapper)
        {
            _mapper = mapper;
            _questionaryService = questionaryService;
        }

        [HttpPost]
        public IActionResult SubmitQuestionary([FromForm]CreateQuestionaryModel questionaryModel)
        {
            try { 
                var res = _questionaryService.CreateQuestionary(questionaryModel);

                if (res != null)
                    return Ok(_mapper.Map<QuestionaryModel>(res));
                else
                    return BadRequest("Ошибка создания опросника. Повторите запрос"); 
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        [HttpGet("{id}")]
        public IActionResult GetQuestionary(Guid id)
        {
            try
            {
                var res = _questionaryService.GetByUniqueId(id);

                if (res != null)
                    return Ok(_mapper.Map<QuestionaryModel>(res));
                else
                    return BadRequest("Ошибка создания опросника. Повторите запрос");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
               
        
        [HttpGet("getunit/{id}")]
        public IActionResult GetQuestionaryUnit(Guid id)
        {
            try
            {
                var res = _questionaryService.GetUnitByUniqueId(id);

                if (res != null)
                    return Ok(res);
                else
                    return BadRequest("Ошибка создания опросника. Повторите запрос");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("acceptquestionary")]
        public async Task<IActionResult> AcceptQuestionary([FromBody] ApproveQuestionaryModel approveQuestionaryModel)
        {
            try
            {
                var res = await _questionaryService.ApproveQuestionary(approveQuestionaryModel.DriverId, approveQuestionaryModel.DriverPass, approveQuestionaryModel.QuestionaryId);
                if (res.IsSuccess)
                    return Ok(res);
                else
                    return BadRequest(res);
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Message = ex.Message,
                    IsSuccess = false
                });
            }
        }

        [HttpGet("all")]
        public IActionResult GetCarQuestionaries(long carId, string? sortColumn, string? sortDirection, int pageNumber, int pageSize)
        {
            var list = _questionaryService.GetListByCarId(carId, sortColumn, sortDirection, pageNumber, pageSize);

            var totalNumber = _questionaryService.GetAll(carId).Count();

            return Ok(new PagedResult<QuestionaryModel>()
            {
                TotalNumber = totalNumber,
                List = _mapper.Map<List<QuestionaryModel>>(list)
            });
        }

        [HttpGet("history")]
        public IActionResult GetAllQuestionaries(string? sortColumn, string? sortDirection, int pageNumber, int pageSize, string? searchColumn, string? search)
        {
            var list = _questionaryService.GetAll(sortColumn, sortDirection, pageNumber, pageSize, searchColumn, search);

            var totalNumber = _questionaryService.GetAll(searchColumn, search).Count();

            return Ok(new PagedResult<QuestionaryModel>()
            {
                TotalNumber = totalNumber,
                List = _mapper.Map<List<QuestionaryModel>>(list)
            });
        }


        [HttpGet("getImages/{questionaryGuid}")]
        public async Task<IActionResult>GetQuestionaryImages(Guid questionaryGuid)
        {
            var list = await _questionaryService.GetImages(questionaryGuid);

            return Ok(list);
        }
    }
}
