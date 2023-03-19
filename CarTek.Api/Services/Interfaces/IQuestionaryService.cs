using CarTek.Api.Model;
using CarTek.Api.Model.Quetionary;
using CarTek.Api.Model.Response;

namespace CarTek.Api.Services.Interfaces
{
    public interface IQuestionaryService
    {
        Questionary CreateQuestionary(CreateQuestionaryModel model);

        Questionary GetByUniqueId(Guid id);

        UnitQuestionaryModel GetUnitByUniqueId(Guid id);

        ICollection<Questionary> GetListByCarId(long carId, string sortColumn, string sortDirection, int pageNumber, int pageSize);

        ICollection<Questionary> GetAll(long carId);

        Task<ICollection<ImageModel>> GetImages(Guid uniqueId);

        Task<ICollection<Questionary>> GetByCarId(string id);

        Task<string> SaveImage(IFormFile file, string path);

        bool DeleteQuestionary(Guid uniqueId);

        Task<ApiResponse> ApproveQuestionary(long driverId, string driverPass, Guid uniqueId);

        IEnumerable<Questionary> GetAll(string searchColumn, string search);

        IEnumerable<Questionary> GetAll(string sortColumn, string sortDirection, int pageNumber, int pageSize,
            string searchColumn, string search);
    }
}
