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

        public Task<ApiResponse> UpdateDriverTask(long taskId, IFormFile file, int status, string comment);
    }
}
