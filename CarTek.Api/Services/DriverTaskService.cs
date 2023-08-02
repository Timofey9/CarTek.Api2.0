using CarTek.Api.DBContext;
using CarTek.Api.Model.Orders;
using CarTek.Api.Model.Response;
using CarTek.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq.Expressions;

namespace CarTek.Api.Services
{
    public class DriverTaskService : IDriverTaskService
    {
        private readonly ILogger<DriverTaskService> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly IAWSS3Service _AWSS3Service;

        public DriverTaskService(ILogger<DriverTaskService> logger, ApplicationDbContext dbContext, IAWSS3Service aWSS3Service)
        {
            _logger = logger;
            _dbContext = dbContext;
            _AWSS3Service = aWSS3Service;
        }

        public IEnumerable<DriverTask> GetDriverTasksAll(DateTime? startDate, DateTime? endDate, long driverId)
        {
            return GetDriverTasksFiltered(0, 0, startDate, endDate, driverId);
        }

        public IEnumerable<DriverTask> GetDriverTasksFiltered(int pageNumber, int pageSize, DateTime? startDate, DateTime? endDate, long driverId)
        {
            pageNumber = pageNumber > 0 ? pageNumber : 1;
            pageSize = pageSize >= 0 ? pageSize : 10;

            var result = new List<DriverTask>();

            try
            {
                Expression<Func<DriverTask, bool>> filterBy = x => x.DriverId == driverId;

                if (startDate != null && endDate != null)
                {
                    var date1 = startDate.Value;
                    var date2 = endDate.Value;
                    filterBy = x => x.DriverId == driverId && x.StartDate.Date >= date1.Date.AddDays(-1) && x.StartDate.Date <= date2.Date;
                }

                Expression<Func<DriverTask, object>> orderBy = x => x.StartDate;

                var tresult = _dbContext.DriverTasks
                    .Include(t => t.Car)
                    .Where(filterBy);

                tresult = tresult.OrderBy(orderBy);

                if (pageSize > 0)
                {
                    tresult = tresult.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                }

                result = tresult.ToList();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось получить список задач");
            }

            return result;
        }

        //TODO: add pagination
        public IEnumerable<DriverTask> GetAllDriverTasks(long driverId)
        {
            return GetDriverTasksAll(null, null, driverId);
        }

        public DriverTask GetDriverTaskById(long driverTaskId)
        {
            var task = _dbContext.DriverTasks
                .Include(dt => dt.Order)
                .ThenInclude(o => o.Material)
                .Include(dt => dt.Car)
                .FirstOrDefault(dt => dt.Id == driverTaskId);

            return task;
        }

        public async Task<ApiResponse> UpdateDriverTask(long taskId, IFormFile file, int status, string comment)
        {
            try
            {
                var task = _dbContext.DriverTasks
                    .Include(dt => dt.Order)
                    .FirstOrDefault(t => t.Id== taskId);  
                
                if(task != null)
                {
                    var taskNote = new DriverTaskNote
                    {
                        DriverTaskId = taskId,
                        Status = (DriverTaskStatus)status,
                        Text = comment
                    };

                    
                    task.Status = (DriverTaskStatus)status;
                    

                    if (file.Length > 1e+7)
                    {
                        throw new UploadedFileException() { ErrorMessage = "Размер файла очень большой" };
                    }

                    var path = task.Order.ClientName + "/" + task.Order.Id + "/" + task.Id + "/" + task.Status.ToString();

                    await _AWSS3Service.UploadFileToS3(file, path, file.FileName, "cartek");

                   
                    _dbContext.DriverTaskNotes.Add(taskNote);

                    _dbContext.Update(task);

                    await _dbContext.SaveChangesAsync();
                }

                //путь до файла в бакете клиент/orderId/taskId/statusId

                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "Статус обновлен"
                };
            }
            catch
            {
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Статус не обновлен"
                };
            }
        }
    }
}
