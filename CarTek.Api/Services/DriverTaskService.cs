using CarTek.Api.DBContext;
using CarTek.Api.Model;
using CarTek.Api.Model.Orders;
using CarTek.Api.Model.Response;
using CarTek.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.IO;
using System.Linq.Expressions;

namespace CarTek.Api.Services
{
    public class DriverTaskService : IDriverTaskService
    {
        private readonly ILogger<DriverTaskService> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly IAWSS3Service _AWSS3Service;
        private readonly INotificationService _notificationService;

        public DriverTaskService(ILogger<DriverTaskService> logger, 
            ApplicationDbContext dbContext, IAWSS3Service aWSS3Service, INotificationService notificationService)
        {
            _logger = logger;
            _dbContext = dbContext;
            _AWSS3Service = aWSS3Service;
            _notificationService = notificationService;
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

                tresult = tresult.OrderByDescending(orderBy);

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
                .Include(dt => dt.Driver)
                .Include(dt => dt.Notes)
                .FirstOrDefault(dt => dt.Id == driverTaskId);

            return task;
        }

        public async Task<ApiResponse> UpdateDriverTask(long taskId, ICollection<IFormFile>? files, int status, string comment)
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
                        Status = (DriverTaskStatus)task.Status,
                        Text = comment,
                        DateCreated = DateTime.UtcNow,
                    };
                    
                    task.Status = (DriverTaskStatus)status;

                    var links = new List<string>();

                    if(files != null)
                    {
                        foreach (var file in files)
                        {
                            if (file.Length > 1e+7)
                            {
                                throw new UploadedFileException() { ErrorMessage = "Размер файла очень большой" };
                            }

                            var path = task.Order.Id + "/" + task.Id + "/" + task.Status.ToString();

                            links.Add("cartek/" + path + "/"+ file.FileName);

                            await _AWSS3Service.UploadFileToS3(file, path, file.FileName, "cartek");
                        }
                    }

                    var stringLinks = JsonConvert.SerializeObject(links);

                    taskNote.S3Links = stringLinks;

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


        public async Task<ApiResponse> AdminUpdateDriverTask(long taskId, long? carId, long? driverId)
        {
            try
            {
                var task = _dbContext.DriverTasks.FirstOrDefault(t => t.Id == taskId);

                if(task != null)
                {
                    if (carId != null)
                    {
                        task.CarId = carId.Value;
                        await _notificationService.SendNotification("Смена транспорта", $"Смена транспорта по задаче #{task.Id}", task.DriverId, true, "http://localhost:3000/driver-dashboard");
                    }

                    if (driverId != null)
                    {
                        await _notificationService.SendNotification("Отмена задачи", $"С вас снята задача. Подробности в личном кабинете", task.DriverId, true, "http://localhost:3000/driver-dashboard");
                        
                        task.DriverId = driverId.Value;

                        await _notificationService.SendNotification("Новая задача", $"На вас назначена новая задача. Подробности в личном кабинете", driverId.Value, true, "http://localhost:3000/driver-dashboard");
                    }

                    _dbContext.Update(task);

                    _dbContext.SaveChanges();

                    return new ApiResponse
                    {
                        IsSuccess = true,
                        Message = "Задача обновлена"
                    };
                }

                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = $"Задача {taskId} не найдена"
                };
            }
            catch(Exception ex)
            {
                _logger.LogError("Ошибка редактирования задачи", ex);

                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Ошибка создания задачи"
                };
            }
        }

        public ApiResponse DeleteDriverTask(long taskId)
        {
            try
            {
                var task = _dbContext.DriverTasks.FirstOrDefault(t => t.Id == taskId);

                if (task != null)
                {
                    _dbContext.DriverTasks.Remove(task);
                    _dbContext.SaveChanges();
                    _notificationService.SendNotification($"Задача удалена", $"Ваша задача на {task.StartDate} была отменена", task.DriverId, true);

                    return new ApiResponse
                    { 
                        IsSuccess = true,
                        Message = "Задача удалена" 
                    };
                }

                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Задача не найдена"
                };
            }
            catch(Exception ex)
            { 
                _logger.LogError(ex.Message, ex);
                return new ApiResponse { IsSuccess = false, Message = "Ошибка удаления задачи"};                
            }
        }

        public TNModel GetTnModel(long driverTaskId)
        {
            try
            {
                var task = _dbContext.DriverTasks
                    .Include(t => t.Order)
                    .ThenInclude(t => t.Material)
                    .Include(t => t.Order)
                    .ThenInclude(t => t.Client)
                    .Include(t => t.Car)
                    .ThenInclude(c => c.Trailer)
                    .Include(dt => dt.Driver)
                    .FirstOrDefault(t => t.Id == driverTaskId);

                if(task != null)
                {
                    var locationA = _dbContext.Addresses.FirstOrDefault(t => t.Id == task.Order.LocationAId);
                    var locationB = _dbContext.Addresses.FirstOrDefault(t => t.Id == task.Order.LocationBId);

                    var driverInfo = task.Driver.FullName;
                    var client = task.Order.Client;
                    var clientInfo = $"{client.ClientName}, ИНН {client.Inn}, КПП {client.Kpp}, {client.ClientAddress}";

                    //TODO: грузоотправитель
                    var result = new TNModel
                    {
                        ClientInfo = clientInfo,
                        DriverInfo = driverInfo,
                        Sender = "ГРУЗООТПРАВИТЕЛЬ",
                        Accepter = "ДОБАВИТЬ КОНТАКТЫ ПРИЕМЩИКА",
                        Material = task.Order.Material.Name,
                        MaterialAmount = $"{task.Volume} {UnitToString(task.Unit)}",
                        CarModel = $"{task.Car.Brand} {task.Car.Model}",
                        CarPlate = task.Car.Plate,
                        TrailerPlate = task.Car.Trailer.Plate,
                        LocationA = locationA.TextAddress,
                        LocationB = locationB.TextAddress
                    };

                    return result;
                }
            }
            catch(Exception ex)
            {
                _logger.LogError("Ошибка сохранения", ex.Message);
            }

            return new TNModel();
        }

        private string UnitToString(Unit unit)
        {
            switch (unit)
            {
                case Unit.t:
                    return "тн";
                case Unit.m3:
                    return "m3";
                case Unit.quantity:
                    return "шт";
                default:
                    return "m3";
            }
        }
    }
}
