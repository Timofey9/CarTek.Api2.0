using CarTek.Api.Model;
using CarTek.Api.Model.Dto;
using CarTek.Api.Model.Orders;
using CarTek.Api.Model.Response;

namespace CarTek.Api.Services.Interfaces
{
    public interface IDriverTaskService
    {
        public DriverTask GetDriverTaskById(long driverTaskId);

        public IEnumerable<DriverTask> GetAllDriverTasks(long driverId);

        public IEnumerable<DriverTask> GetDriverTasksAll(DateTime? startDate, DateTime? endDate, long driverId);

        public IEnumerable<DriverTask> GetDriverTasksFiltered(int pageNumber, int pageSize, DateTime? startDate, DateTime? endDate, long driverId);

        public Task<ApiResponse> UpdateDriverTask(long taskId, ICollection<IFormFile>? file, int status, string comment);

        public Task<ApiResponse> AdminUpdateDriverTask(long taskId, long? carId, long? driverId, string? adminComment);

        public ApiResponse DeleteDriverTask(long taskId);
        
        public TNModel GetTnModel(long driverTaskId);

        public ApiResponse StartDocument(FillDocumentModel model);

        public ApiResponse FinalizeDocument(FillDocumentModel model);

        List<DriverTaskOrderModel> MapAndExtractLocationsInfo(IEnumerable<DriverTask> listToConvert);

        public void DriverTaskExportModelSetLocations(DriverTaskExportModel model);
    }
}
