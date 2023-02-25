using CarTek.Api.Model;
using CarTek.Api.Model.Quetionary;
using CarTek.Api.Model.Response;

namespace CarTek.Api.Services.Interfaces
{
    public interface IQuestionaryService
    {
        Task<Questionary> CreateQuestionary(CreateQuestionaryModel model);

        Questionary GetByUniqueId(Guid id);

        Task<ICollection<Questionary>> GetByCarPlate(string plate);

        Task<ICollection<Questionary>> GetByCarId(string id);

        Task<string> SaveImage(IFormFile file, string path);

        Task<ApiResponse> ApproveQuestionary(long driverId, string driverPass, Guid uniqueId);
    }
}
