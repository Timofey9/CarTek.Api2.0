using AutoMapper;
using CarTek.Api.Model.Dto;
using CarTek.Api.Model.Quetionary;
using CarTek.Api.Model.Response;
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
        public async Task<IActionResult> SubmitQuestionary([FromForm]CreateQuestionaryModel questionaryModel)
        {
            try { 
                var res = await _questionaryService.CreateQuestionary(questionaryModel);

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
    }
}
