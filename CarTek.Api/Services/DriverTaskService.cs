using AutoMapper;
using CarTek.Api.DBContext;
using CarTek.Api.Model;
using CarTek.Api.Model.Dto;
using CarTek.Api.Model.Orders;
using CarTek.Api.Model.Response;
using CarTek.Api.Services.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Linq.Expressions;

namespace CarTek.Api.Services
{
    public class DriverTaskService : IDriverTaskService
    {
        private readonly ILogger<DriverTaskService> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly IAWSS3Service _AWSS3Service;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;

        public DriverTaskService(ILogger<DriverTaskService> logger,
            ApplicationDbContext dbContext, IAWSS3Service aWSS3Service, INotificationService notificationService, IMapper mapper)
        {
            _logger = logger;
            _dbContext = dbContext;
            _AWSS3Service = aWSS3Service;
            _notificationService = notificationService;
            _mapper = mapper;
        }

        public IEnumerable<DriverTask> GetDriverTasksAll(DateTime? startDate, DateTime? endDate, long driverId, string? searchBy, string? searchString)
        {
            return GetDriverTasksFiltered(0, 0, startDate, endDate, driverId, searchBy, searchString);
        }

        public IEnumerable<DriverTask> GetDriverTasksFiltered(int pageNumber, int pageSize, DateTime? startDate, DateTime? endDate, long driverId, string? searchBy, string? searchString)
        {
            pageNumber = pageNumber > 0 ? pageNumber : 1;
            pageSize = pageSize >= 0 ? pageSize : 10;

            var result = new List<DriverTask>();

            try
            {
                Expression<Func<DriverTask, bool>> filterBy = x => x.DriverId == driverId;

                if (startDate == null || endDate == null)
                {
                    return result;
                }

                var date1 = startDate.Value.Date;
                var date2 = endDate.Value.Date;

                filterBy = x => x.DriverId == driverId &&
                        x.StartDate.AddHours(4).Date >= date1
                        && x.StartDate.AddHours(4).Date <= date2;

                if (!string.IsNullOrEmpty(searchBy) && !string.IsNullOrEmpty(searchString))
                {
                    switch (searchBy)
                    {
                        case "clientName":
                            filterBy = x => x.DriverId == driverId && x.Order.ClientName.ToLower().Contains(searchString.ToLower().Trim())
                            && x.StartDate.AddHours(4).Date >= date1
                            && x.StartDate.AddHours(4).Date <= date2;

                            break;
                        case "material":
                            filterBy = x => x.DriverId == driverId && x.Order.Material.Name.ToLower().Contains(searchString.ToLower().Trim())
                            && x.StartDate.AddHours(4).Date >= date1
                            && x.StartDate.AddHours(4).Date <= date2;
                            break;
                        default:
                            break;
                    }
                }


                Expression<Func<DriverTask, object>> orderBy = x => x.StartDate;

                var tresult = _dbContext.DriverTasks
                    .Include(t => t.Car)
                    .Include(t => t.Order)
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

        public List<DriverTaskOrderModel> MapAndExtractLocationsInfo(IEnumerable<DriverTask> listToConvert)
        {
            var mappedResult = new List<DriverTaskOrderModel>();

            try
            {
                var list = listToConvert.ToList();

                mappedResult = _mapper.Map<List<DriverTaskOrderModel>>(list);

                for (var i = 0; i < list.Count; i++)
                {
                    Address locationA = new Address();
                    Address locationB = new Address();

                    if (list[i].Order?.LocationAId != null)
                    {
                        locationA = _dbContext.Addresses.FirstOrDefault(t => t.Id == list[i].Order.LocationAId);
                    }

                    if (list[i].Order?.LocationBId != null)
                    {
                        locationB = _dbContext.Addresses.FirstOrDefault(t => t.Id == list[i].Order.LocationBId);
                    }

                    mappedResult[i].LocationA = locationA;
                    mappedResult[i].LocationB = locationB;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Не удалось получить адреса, {ex.Message}");
            }

            return mappedResult;
        }

        public DriverTask GetDriverTaskById(long driverTaskId)
        {
            var task = _dbContext.DriverTasks
                .Include(dt => dt.Order)
                .ThenInclude(o => o.Material)
                .Include(dt => dt.Order)
                .ThenInclude(o => o.Client)
                .Include(dt => dt.Car)
                .Include(dt => dt.Driver)
                .Include(dt => dt.Notes)
                .Include(dt => dt.SubTasks)
                .ThenInclude(st => st.Notes)
                .FirstOrDefault(dt => dt.Id == driverTaskId);

            return task;
        }

        public void DriverTaskExportModelSetLocations(DriverTaskExportModel model)
        {
            try
            {
                Address locationA = new Address();
                Address locationB = new Address();

                if (model?.Order?.LocationAId != null)
                {
                    locationA = _dbContext.Addresses.FirstOrDefault(t => t.Id == model.Order.LocationAId);
                }

                if (model?.Order?.LocationBId != null)
                {
                    locationB = _dbContext.Addresses.FirstOrDefault(t => t.Id == model.Order.LocationBId);
                }

                model.LocationA = locationA;
                model.LocationB = locationB;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Не удалось получить адреса, {ex.Message}");
            }
        }

        public async Task<ApiResponse> UpdateDriverTask(long taskId, ICollection<IFormFile>? files, int status, string comment)
        {
            try
            {
                var task = _dbContext.DriverTasks
                    .Include(dt => dt.Order)
                    .FirstOrDefault(t => t.Id == taskId);

                if (task != null)
                {
                    var taskNote = new DriverTaskNote
                    {
                        DriverTaskId = taskId,
                        Status = (DriverTaskStatus)status,
                        Text = string.IsNullOrEmpty(comment) ? " " : comment,
                        DateCreated = DateTime.UtcNow,
                    };

                    task.Status = (DriverTaskStatus)status;

                    var links = new List<string>();

                    if (files != null)
                    {
                        foreach (var file in files)
                        {
                            var path = task.Order.Id + "/" + task.Id + "/" + task.Status.ToString();

                            links.Add("cartek/" + path + "/" + file.FileName);

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
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);

                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Статус не обновлен"
                };
            }
        }

        public async Task<ApiResponse> AdminUpdateDriverTask(long taskId, long? carId, long? driverId, string? adminComment, DateTime? startDate, ShiftType? shift, long? orderId)
        {
            try
            {
                var task = _dbContext.DriverTasks.FirstOrDefault(t => t.Id == taskId);

                if (task != null)
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

                    if (orderId != null)
                    {
                        task.OrderId = orderId.Value;
                        var subtasks = _dbContext.SubTasks.Where(t => t.DriverTaskId == taskId);
                        foreach(var subtask in subtasks)
                        {
                            subtask.OrderId = orderId.Value;
                        }

                        _dbContext.UpdateRange(subtasks);
                    }

                    if (!string.IsNullOrEmpty(adminComment))
                    {
                        task.AdminComment = adminComment;
                        await _notificationService.SendNotification("Комментарий", $"Обновлен комментарий по задаче от {task.StartDate:dd.MM.yyyy}", task.DriverId, true, "http://localhost:3000/driver-dashboard");
                    }

                    if (startDate != null && startDate.Value.Date != task.StartDate.Date)
                    {
                        await _notificationService.SendNotification("Смена даты", $"Обновлена дата по задаче от {task.StartDate:dd.MM.yyyy}", task.DriverId, true, "http://localhost:3000/driver-dashboard");
                        task.StartDate = startDate.Value;
                    }

                    if (shift != null && shift.Value != task.Shift)
                    {
                        await _notificationService.SendNotification("Изменена смена", $"Изменена смена по задаче от {task.StartDate:dd.MM.yyyy}", task.DriverId, true, "http://localhost:3000/driver-dashboard");
                        task.Shift = shift.Value;
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
            catch (Exception ex)
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
                var task = _dbContext.DriverTasks.Include(dt => dt.TN).FirstOrDefault(t => t.Id == taskId);

                if (task != null)
                {
                    _dbContext.DriverTasks.Remove(task);

                    _dbContext.SubTasks.RemoveRange(_dbContext.SubTasks.Include(st => st.TN).Include(st => st.Notes).Where(st => st.DriverTaskId == taskId));
                    _dbContext.DriverTaskNotes.RemoveRange(_dbContext.DriverTaskNotes.Where(dn => dn.DriverTaskId == taskId));

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
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new ApiResponse { IsSuccess = false, Message = "Ошибка удаления задачи" };
            }
        }

        public TNModel GetTnModel(long driverTaskId)
        {
            try
            {
                var tn = _dbContext.TNs.FirstOrDefault(t => t.DriverTaskId == driverTaskId);

                var task = _dbContext.DriverTasks
                    .Include(t => t.Order)
                    .ThenInclude(t => t.Material)
                    .Include(t => t.Order)
                    .ThenInclude(t => t.Client)
                    .Include(t => t.Car)
                    .ThenInclude(c => c.Trailer)
                    .Include(dt => dt.Driver)
                    .FirstOrDefault(t => t.Id == driverTaskId);

                if (tn != null)
                {
                    var locationA = _dbContext.Addresses.FirstOrDefault(t => t.Id == tn.LocationAId);
                    var locationB = _dbContext.Addresses.FirstOrDefault(t => t.Id == tn.LocationBId);

                    var driverInfo = tn.DriverTask.Driver.FullName;

                    var gp = _dbContext.Clients.FirstOrDefault(t => t.Id == tn.GpId);
                    var go = _dbContext.Clients.FirstOrDefault(t => t.Id == tn.GoId);

                    var gpInfo = $"{gp?.ClientName}, ИНН {gp?.Inn}";
                    var goInfo = $"{go?.ClientName}, ИНН {go?.Inn}";

                    //TODO: грузоотправитель
                    var result = new TNModel
                    {
                        GoInfo = goInfo,
                        DriverInfo = driverInfo,
                        Date = tn.DriverTask.StartDate,
                        Number = tn.Number,
                        GpInfo = gpInfo,
                        Accepter = "",
                        Unit = UnitToString(tn.Unit),
                        LoadVolume = tn.LoadVolume.ToString(),
                        UnloadVolume = tn.UnloadVolume.ToString(),
                        Material = tn.DriverTask.Order.Material.Name,
                        MaterialAmount = $"{tn.LoadVolume} {UnitToString(tn.Unit)}",
                        CarModel = $"{tn.DriverTask.Car.Brand} {tn.DriverTask.Car.Model}",
                        CarPlate = tn.DriverTask.Car.Plate,
                        TrailerPlate = tn.DriverTask.Car?.Trailer?.Plate,
                        LocationA = locationA?.TextAddress,
                        LocationB = locationB?.TextAddress,
                        PickUpArrivalTime = $"{tn.PickUpArrivalDate?.ToString("dd.MM.yyyy")} {tn.PickUpArrivalTime}",
                        PickUpDepartureTime = $"{tn.PickUpDepartureDate?.ToString("dd.MM.yyyy")} {tn.PickUpDepartureTime}",
                        DropOffArrivalTime = $"{tn.DropOffArrivalDate?.ToString("dd.MM.yyyy")} {tn.DropOffArrivalTime}",
                        DropOffDepartureTime = $"{tn.DropOffDepartureDate?.ToString("dd.MM.yyyy")} {tn.DropOffDepartureTime}",
                    };

                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка сохранения: {ex.Message}", ex.Message);
            }

            return new TNModel();
        }

        private string UnitToString(Unit? unit)
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

        public ApiResponse StartDocument(FillDocumentModel model)
        {
            try
            {
                var task = _dbContext.DriverTasks
                    .Include(t => t.TN)
                    .FirstOrDefault(dt => dt.Id == model.DriverTaskId);

                if (task != null)
                {
                    var Tn = new TN
                    {
                        Number = model.Number,
                        GoId = model.GoId,
                        GpId = model.GpId,
                        LoadVolume = model.LoadVolume,
                        Unit = model.Unit,
                        LocationAId = model.LocationAId,
                        LocationBId = model.LocationBId,
                        PickUpArrivalDate = model.PickUpArrivalDate,
                        PickUpArrivalTime = model.PickUpArrivalTime,
                        PickUpDepartureDate = model.PickUpDepartureDate,
                        PickUpDepartureTime = model.PickUpDepartureTime,
                        DriverTaskId = task.Id,
                        DriverId = task.DriverId
                    };

                    if (model.IsSubtask)
                    {
                        Tn.SubTaskId = model.SubTaskId;
                        Tn.DriverTaskId = null;
                        _dbContext.TNs.Add(Tn);
                    }
                    else
                    {
                        if (task.TN == null)
                        {
                            _dbContext.TNs.Add(Tn);
                        }
                        else
                        {
                            var updTn = task.TN;
                            _dbContext.Update(updTn).CurrentValues.SetValues(new {
                                Number = model.Number,
                                GoId = model.GoId,
                                GpId = model.GpId,
                                LoadVolume = model.LoadVolume,
                                Unit = model.Unit,
                                LocationAId = model.LocationAId,
                                LocationBId = model.LocationBId,
                                PickUpArrivalDate = model.PickUpArrivalDate,
                                PickUpArrivalTime = model.PickUpArrivalTime,
                                PickUpDepartureDate = model.PickUpDepartureDate,
                                PickUpDepartureTime = model.PickUpDepartureTime,
                                DriverTaskId = task.Id,
                                DriverId = task.DriverId
                            });
                        }
                    }

                    _dbContext.SaveChanges();

                    return new ApiResponse
                    {
                        IsSuccess = true,
                        Message = "Данные сохранены"
                    };
                }

                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Задача не найдена"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка заполнения ТН.{ex.Message}");
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Ошибка сохранения данных"
                };
            }
        }

        public ApiResponse FinalizeDocument(FillDocumentModel model)
        {
            try
            {
                if (model.IsSubtask)
                {
                    var TN = _dbContext.TNs.FirstOrDefault(t => t.SubTaskId == model.SubTaskId);

                    if (TN != null)
                    {
                        TN.UnloadVolume = model.UnloadVolume;
                        TN.DropOffArrivalDate = model.DropOffArrivalDate;
                        TN.LocationBId = model.LocationBId;
                        TN.DropOffDepartureDate = model.DropOffDepartureDate;
                        TN.DropOffArrivalTime = model.DropOffArrivalTime;
                        TN.DropOffDepartureTime = model.DropOffDepartureTime;

                        _dbContext.Update(TN);
                    }

                    _dbContext.SaveChanges();

                    return new ApiResponse
                    {
                        IsSuccess = true,
                        Message = "Данные сохранены"
                    };
                }

                var task = _dbContext.DriverTasks
                    .Include(t => t.TN)
                    .FirstOrDefault(dt => dt.Id == model.DriverTaskId);

                if (task != null)
                {
                    if (task.TN != null)
                    {
                        task.TN.UnloadVolume = model.UnloadVolume;
                        task.TN.DropOffArrivalDate = model.DropOffArrivalDate;
                        task.TN.LocationBId = model.LocationBId;
                        task.TN.DropOffDepartureDate = model.DropOffDepartureDate;
                        task.TN.DropOffArrivalTime = model.DropOffArrivalTime;
                        task.TN.DropOffDepartureTime = model.DropOffDepartureTime;

                        _dbContext.Update(task.TN);
                    }

                    _dbContext.SaveChanges();

                    return new ApiResponse
                    {
                        IsSuccess = true,
                        Message = "Данные сохранены"
                    };
                }

                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Задача не найдена"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка заполнения ТН.{ex.Message}");
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Ошибка сохранения данных"
                };
            }
        }

        public ApiResponse CreateSubTask(long driverTaskId)
        {
            try
            {
                var dt = _dbContext.DriverTasks.Include(d => d.SubTasks).FirstOrDefault(t => t.Id == driverTaskId);

                if (dt == null)
                    return new ApiResponse
                    {
                        IsSuccess = false,
                        Message = "Задача не существует"
                    };

                var subTask = new SubTask
                {
                    DriverTaskId = driverTaskId,
                    Status = DriverTaskStatus.Confirmed,
                    SequenceNumber = dt.SubTasks.Count + 1,
                    OrderId = dt.OrderId
                };

                dt.SubTasksCount++;

                _dbContext.SubTasks.Add(subTask);
                _dbContext.DriverTasks.Update(dt);
                _dbContext.SaveChanges();

                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "Создана подзадача"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Невозможно создать позадачу. {ex.Message}");
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Задача не создана"
                };
            }
        }

        public async Task<ApiResponse> SubmitDtNote(long taskId, ICollection<IFormFile>? files, string comment)
        {
            try {
                var task = _dbContext.DriverTasks.FirstOrDefault(t => t.Id == taskId);

                if (task != null)
                {
                    var taskNote = new DriverTaskNote
                    {
                        DriverTaskId = taskId,
                        Status = task.Status,
                        Text = string.IsNullOrEmpty(comment) ? " " : comment,
                        DateCreated = DateTime.UtcNow,
                    };

                    var links = new List<string>();

                    if (files != null)
                    {
                        foreach (var file in files)
                        {
                            var path = task.OrderId + "/" + task.Id + "/" + task.Status.ToString();

                            links.Add("cartek/" + path + "/" + file.FileName);

                            await _AWSS3Service.UploadFileToS3(file, path, file.FileName, "cartek");
                        }
                    }

                    var stringLinks = JsonConvert.SerializeObject(links);

                    taskNote.S3Links = stringLinks;

                    _dbContext.DriverTaskNotes.Add(taskNote);

                    await _dbContext.SaveChangesAsync();

                    return new ApiResponse
                    {
                        IsSuccess = true,
                        Message = "Комментарий добавлен"
                    };

                }

                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Комментарий не добавлен"
                };
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message, ex);

                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Комментарий не добавлен"
                };
            }
        }

        public async Task<ApiResponse> UpdateDriverSubTask(long taskId, ICollection<IFormFile>? files, int status, string comment)
        {
            try
            {
                var task = _dbContext.SubTasks.FirstOrDefault(t => t.Id == taskId);

                if (task != null)
                {
                    var taskNote = new DriverTaskNote
                    {
                        SubTaskId = taskId,
                        Status = (DriverTaskStatus)task.Status,
                        Text = string.IsNullOrEmpty(comment) ? " " : comment,
                        DateCreated = DateTime.UtcNow,
                    };

                    task.Status = (DriverTaskStatus)(status + 1);

                    var links = new List<string>();

                    if (files != null)
                    {
                        foreach (var file in files)
                        {
                            var path = task.OrderId + "/" + task.Id + "/" + task.Status.ToString();

                            links.Add("cartek/" + path + "/" + file.FileName);

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
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);

                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Статус не обновлен"
                };
            }
        }

        public IEnumerable<SubTaskModel> GetSubTasks(long driverTaskId)
        {
            var tasks = _dbContext.SubTasks.Where(t => t.DriverTaskId == driverTaskId);
            var map = _mapper.Map<List<SubTaskModel>>(tasks);

            return map;
        }
    }
}
