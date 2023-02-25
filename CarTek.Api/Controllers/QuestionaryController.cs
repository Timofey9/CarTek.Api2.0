using AutoMapper;
using CarTek.Api.Model.Dto;
using CarTek.Api.Model.Quetionary;
using CarTek.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

                return Ok(_mapper.Map<QuestionaryModel>(res));
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
