using CarTek.Api.Model;
using CarTek.Api.Model.Quetionary;

namespace CarTek.Api.Services.Interfaces
{
    public interface IQuestionaryService
    {
        Task<Questionary> CreateQuestionary(CreateQuestionaryModel model);

        Task<string> SaveImage(IFormFile file, string path);
    }
}
