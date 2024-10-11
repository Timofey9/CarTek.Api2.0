using CarTek.Api.Model;
using CarTek.Api.Model.Dto;

namespace CarTek.Api.Services.Interfaces
{
    public interface ITnService
    {
        //Весь список ТН между дат
        IEnumerable<TNModel> GetAllPagination(string sortColumn, string sortDirection, int pageNumber, int pageSize, string searchColumn, string search, DateTime startDate, DateTime endDate);
        
        //Со страницами + Поиск по номеру
        IEnumerable<TNModel> GetAll(string? searchColumn, string? search, DateTime startDate, DateTime endDate);

        IEnumerable<DriverSalaryTableModel> GetAllGrouped(string? searchColumn, string? search, DateTime startDate, DateTime endDate);
    }
}
