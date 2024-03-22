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

            var considerDriver = driverId != 0;

            try
            {
                Expression<Func<DriverTask, bool>> filterBy = x => !considerDriver || x.DriverId == driverId;

                if (startDate == null || endDate == null)
                {
                    return result;
                }

                var date1 = startDate.Value.Date;
                var date2 = endDate.Value.Date;

                filterBy = x => (!considerDriver || x.DriverId == driverId) &&
                        x.StartDate.AddHours(4).Date >= date1
                        && x.StartDate.AddHours(4).Date <= date2;

                if (!string.IsNullOrEmpty(searchBy) && !string.IsNullOrEmpty(searchString))
                {
                    switch (searchBy)
                    {
                        case "clientName":
                            filterBy = x => (!considerDriver || x.DriverId == driverId) && x.Order.ClientName.ToLower().Contains(searchString.ToLower().Trim())
                            && x.StartDate.AddHours(4).Date >= date1
                            && x.StartDate.AddHours(4).Date <= date2;

                            break;
                        case "material":
                            filterBy = x => (!considerDriver || x.DriverId == driverId) && x.Order.Material.Name.ToLower().Contains(searchString.ToLower().Trim())
                            && x.StartDate.AddHours(4).Date >= date1
                            && x.StartDate.AddHours(4).Date <= date2;
                            break;
                        default:
                            break;
                    }
                }

                var tresult = _dbContext.DriverTasks
                    .Include(t => t.Car)
                    .Include(t => t.SubTasks)
                    .Include(t => t.Order)
                    .ThenInclude(t => t.Material)
                    .Include(t => t.Notes)
                    .Where(filterBy);

                tresult = tresult.OrderByDescending(x => x.StartDate);

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


        public IEnumerable<DriverTaskReportModel> GetDriverTasksBetweenDates(DateTime startDate, DateTime endDate, string? searchBy, string? searchString)
        {
            var result = new List<DriverTask>();

            var mappedResult = new List<DriverTaskReportModel>();
            try
            {
                var date1 = startDate.Date;
                var date2 = endDate.Date;

                Expression<Func<DriverTask, bool>> filterBy = x =>
                        x.StartDate.AddHours(4).Date >= date1
                        && x.StartDate.AddHours(4).Date <= date2;

                Expression<Func<DriverTask, object>> orderBy = x => x.StartDate;

                if (!string.IsNullOrEmpty(searchBy) && !string.IsNullOrEmpty(searchString))
                {
                    switch (searchBy)
                    {
                        case "clientName":
                            filterBy = x => x.Order.ClientName.ToLower().Contains(searchString.ToLower().Trim())
                            && x.StartDate.AddHours(4).Date >= date1
                            && x.StartDate.AddHours(4).Date <= date2;

                            break;
                        case "material":
                            filterBy = x => x.Order.Material.Name.ToLower().Contains(searchString.ToLower().Trim())
                            && x.StartDate.AddHours(4).Date >= date1
                            && x.StartDate.AddHours(4).Date <= date2;
                            break;
                        default:
                            break;
                    }
                }

                var tresult = _dbContext.DriverTasks
                    .Include(t => t.Car)
                    .Include(dt => dt.Driver)
                    .Include(t => t.Order)
                    .ThenInclude(t => t.Material)
                    .Where(filterBy).AsEnumerable();

                var ordered = tresult.OrderBy(t => t.Car.Plate.Substring(1, 3), new SemiNumericComparer());

                result = ordered.ToList();

                foreach (var task in result)
                {
                    var gp = _dbContext.Clients.FirstOrDefault(t => t.Id == task.Order.GpId);

                    var client = task.Order.Service == ServiceType.Supply ? gp?.ClientName : task.Order.ClientName;

                    var mappedTask = new DriverTaskReportModel
                    {
                        Driver = task.Driver.FullName,
                        Plate = task.Car.Plate.ToUpper(),
                        Service = task.Order.Service == ServiceType.Supply ? "Поставка" : "Перевозка",
                        Go = task?.Order?.ClientName,
                        Gp = gp?.ClientName,
                        Client = client,
                        LocationA = _dbContext.Addresses.FirstOrDefault(t => t.Id == task.Order.LocationAId)?.TextAddress,
                        LocationB = _dbContext.Addresses.FirstOrDefault(t => t.Id == task.Order.LocationBId)?.TextAddress,
                        Material = task.Order.Material?.Name,
                        Shift = task.Shift,
                        Status = task.Status,
                        OrderComment = task.Order.Note,
                        TaskComment = task.AdminComment,
                        IsCanceled = task.IsCanceled
                    };

                    bool add = false;

                    if (!string.IsNullOrEmpty(searchBy) && !string.IsNullOrEmpty(searchString))
                    {
                        switch (searchBy)
                        {
                            case "addressA":
                                if ((!string.IsNullOrEmpty(mappedTask.LocationA) && mappedTask.LocationA.ToLower().Contains(searchString.ToLower())))
                                {
                                    mappedResult.Add(mappedTask);
                                }
                                break;
                            case "addressB":
                                if ((!string.IsNullOrEmpty(mappedTask.LocationB) && mappedTask.LocationA.ToLower().Contains(searchString.ToLower())))
                                {
                                    mappedResult.Add(mappedTask);
                                }
                                break;
                            default:
                                mappedResult.Add(mappedTask);
                                break;
                        }
                    }
                    else
                    {
                        mappedResult.Add(mappedTask);
                    }

                    mappedResult.Add(mappedTask);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось получить список задач");
            }

            return mappedResult;
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
                    var driverTask = list[i];

                    var clientId = driverTask.Order.Service == ServiceType.Supply ? driverTask.Order.GpId : driverTask.Order.ClientId;

                    var client = _dbContext.Clients.FirstOrDefault(t => t.Id == clientId);

                    string price = "";
                    string driverPrice = "";

                    if (client != null)
                    {
                        var unit = UnitToString(client.ClientUnit);

                        if (client.FixedPrice == null)
                        {
                            price = driverTask.Order.Price + $" руб/{unit}";
                            driverPrice = driverTask.Order.DriverPrice + $" руб/{unit}";
                        }
                        else
                        {
                            price = client.FixedPrice + " руб";
                            driverPrice = client.FixedPrice + " руб";
                        }
                    }

                    driverTask.Order.DriverTasks = null;

                    Model.Address locationA = new Model.Address();
                    Model.Address locationB = new Model.Address();

                    if (driverTask.Order?.LocationAId != null)
                    {
                        locationA = _dbContext.Addresses.FirstOrDefault(t => t.Id == driverTask.Order.LocationAId);
                    }

                    if (driverTask.Order?.LocationBId != null)
                    {
                        locationB = _dbContext.Addresses.FirstOrDefault(t => t.Id == driverTask.Order.LocationBId);
                    }

                    mappedResult[i].LocationA = locationA;
                    mappedResult[i].LocationB = locationB;

                    mappedResult[i].Material = driverTask.Order?.Material?.Name;

                    mappedResult[i].Price = price;
                    mappedResult[i].DriverPrice = driverPrice;

                    if (mappedResult[i].SubTasksCount > 0 && mappedResult[i].SubTasks != null)
                    {
                        var subTasks = mappedResult[i].SubTasks.Select(t => t);
                        foreach(var st in subTasks)
                        {
                            st.DriverTask.Order = null;
                        }
                        mappedResult[i].SubTasks = subTasks.OrderBy(t => t.SequenceNumber).ToList();
                    }

                    if(driverTask.Notes != null)
                    {
                        mappedResult[i].LastNote = driverTask.Notes.MaxBy(t => t.DateCreated);
                    }
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
                .Include(dt => dt.Order)
                .ThenInclude(o => o.ExternalTransporter)
                .Include(dt => dt.Order)
                .ThenInclude(o => o.DriverTasks).ThenInclude(dts => dts.Car)
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
                Model.Address locationA = new Model.Address();
                Model.Address locationB = new Model.Address();

                if (model?.Order?.LocationAId != null)
                {
                    locationA = _dbContext.Addresses.FirstOrDefault(t => t.Id == model.Order.LocationAId);
                }

                if (model?.Order?.LocationBId != null)
                {
                    locationB = _dbContext.Addresses.FirstOrDefault(t => t.Id == model.Order.LocationBId);
                }

                var clientId = model.Order.Service == ServiceType.Supply ? model.Order.GpId : model.Order.Client.Id;

                var client = _dbContext.Clients.FirstOrDefault(t => t.Id == clientId);

                var gp = _dbContext.Clients.FirstOrDefault(t => t.Id == model.Order.GpId);

                if (gp != null)
                {
                    model.Order.Gp = _mapper.Map<ClientModel>(gp);
                }

                string price = "";
                string driverPrice = "";

                if (client != null)
                {
                    var unit = UnitToString(model.Order.LoadUnit);

                    if (client.FixedPrice == null)
                    {
                        price = model.Order.Price + $" руб/{unit}";
                        driverPrice = model.Order.DriverPrice + $" руб/{unit}";
                    }
                    else
                    {
                        price = client.FixedPrice + " руб";
                        driverPrice = client.FixedPrice + $" руб/{unit}";
                    }

                    model.OrderCustomer = _mapper.Map<ClientModel>(client);
                }

                model.LocationA = locationA;
                model.LocationB = locationB;
                model.Price = price;
                model.DriverPrice = driverPrice;

                string carPlates = string.Join(", ", model.Order.DriverTasks.Select(p => $"{p.Car.Plate}"));

                model.TransportAmountMessage = $"По заявке назначено {model.Order.DriverTasks.Count} а.м. Гос. номера: {carPlates}";

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Не удалось получить адреса, {ex.Message}");
            }
        }        
        
        public void DriverTaskExportModelSetLocations(DriverTaskSubTaskModel model)
        {
            try
            {
                Model.Address locationA = new Model.Address();
                Model.Address locationB = new Model.Address();

                if (model?.Order?.LocationAId != null)
                {
                    locationA = _dbContext.Addresses.FirstOrDefault(t => t.Id == model.Order.LocationAId);
                }

                if (model?.Order?.LocationBId != null)
                {
                    locationB = _dbContext.Addresses.FirstOrDefault(t => t.Id == model.Order.LocationBId);
                }

                var clientId = model.Order.Service == ServiceType.Supply ? model.Order.GpId : model.Order.Client.Id;

                var client = _dbContext.Clients.FirstOrDefault(t => t.Id == clientId);

                var gp = _dbContext.Clients.FirstOrDefault(t => t.Id == model.Order.GpId);

                if (gp != null)
                {
                    model.Order.Gp = _mapper.Map<ClientModel>(gp);
                }

                string price = "";
                string driverPrice = "";

                if (client != null)
                {
                    var unit = UnitToString(model.Order.LoadUnit);

                    if (client.FixedPrice == null)
                    {
                        price = model.Order.Price + $" руб/{unit}";
                        driverPrice = model.Order.DriverPrice + $" руб/{unit}";
                    }
                    else
                    {
                        price = client.FixedPrice + " руб";
                        driverPrice = client.FixedPrice + $" руб/{unit}";
                    }

                    model.OrderCustomer = _mapper.Map<ClientModel>(client);
                }

                model.LocationA = locationA;
                model.LocationB = locationB;
                model.Price = price;
                model.DriverPrice = driverPrice;
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

                    if (status > 9)
                    {
                        task.Status = DriverTaskStatus.Done;
                    }
                    else if (status == 8)
                    {
                        task.Status = DriverTaskStatus.Done;
                    }
                    else
                    {
                        task.Status = (DriverTaskStatus)status;
                    }

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

                    _dbContext.SaveChanges();
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
                _logger.LogError($"Ошибка обновления статуса: {ex.Message}", ex);

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
                        foreach (var subtask in subtasks)
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
                    var subTasks = _dbContext.SubTasks
                        .Include(st => st.TN)
                        .Include(st => st.Notes)
                        .Where(st => st.DriverTaskId == taskId);

                    if (subTasks != null && subTasks.Count() > 0)
                    {
                        foreach (var subTask in subTasks)
                        {
                            if (subTask.TN != null)
                                _dbContext.TNs.Remove(subTask.TN);
                            if (subTask.Notes != null)
                            {
                                _dbContext.DriverTaskNotes.RemoveRange(subTask.Notes);
                            }

                            _dbContext.Remove(subTask);
                        }
                    }

                    _dbContext.DriverTaskNotes.RemoveRange(_dbContext.DriverTaskNotes.Where(dn => dn.DriverTaskId == taskId));

                    _dbContext.TNs.RemoveRange(_dbContext.TNs.Where(tn => tn.DriverTaskId == taskId));

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
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return new ApiResponse { IsSuccess = false, Message = "Ошибка удаления задачи" };
            }
        }

        public EditTNModel GetEditTnModel(long driverTaskId, bool isSubtask = false)
        {
            try
            {
                var result = new EditTNModel();
                TN tn;
                if (isSubtask)
                {
                    tn = _dbContext.TNs
                        .Include(t => t.Material)
                        .Include(tn => tn.LocationA)
                        .Include(tn => tn.LocationB)
                        .FirstOrDefault(t => t.SubTaskId == driverTaskId);

                    if (tn != null)
                    {
                        //var driverInfo = tn.DriverTask.Driver.FullName;

                        var gp = _dbContext.Clients.FirstOrDefault(t => t.Id == tn.GpId);
                        var go = _dbContext.Clients.FirstOrDefault(t => t.Id == tn.GoId);

                        var gpInfo = $"{gp?.ClientName}, ИНН {gp?.Inn}";
                        var goInfo = $"{go?.ClientName}, ИНН {go?.Inn}";

                        //TODO: грузоотправитель
                        result = new EditTNModel
                        {
                            IsOriginalReceived = tn.IsOrginalReceived ?? false,
                            Go = new ClientModel
                            {
                                ClientName = go?.ClientName,
                                ClientAddress = go?.ClientAddress,
                                Id = go.Id,
                                Inn = go?.Inn
                            },
                            Gp = new ClientModel
                            {
                                ClientName = gp?.ClientName,
                                ClientAddress = gp?.ClientAddress,
                                Id = gp.Id,
                                Inn = gp?.Inn
                            },
                            //DriverInfo = driverInfo,
                            //Date = tn.DriverTask.StartDate,
                            Number = tn.Number,
                            Unit = UnitToString(tn.Unit),
                            Unit2 = UnitToString(tn.Unit2),
                            UnloadUnit = UnitToString(tn.UnloadUnit),
                            UnloadUnit2 = UnitToString(tn.UnloadUnit2),
                            LoadVolume = tn.LoadVolume.ToString(),
                            LoadVolume2 = tn.LoadVolume2?.ToString(),
                            UnloadVolume = tn.UnloadVolume?.ToString(),
                            UnloadVolume2 = tn.UnloadVolume2?.ToString(),
                            Material = new MaterialModel
                            {
                                Id = tn.Material.Id,
                                Name = tn.Material.Name
                            },
                            MaterialAmount = $"{tn.LoadVolume} {UnitToString(tn.Unit)}",
                            //CarModel = $"{tn.DriverTask.Car.Brand} {tn.DriverTask.Car.Model}",
                            //CarPlate = tn.DriverTask.Car.Plate,
                            //TrailerPlate = tn.DriverTask.Car?.Trailer?.Plate,
                            LocationA = _mapper.Map<AddressModel>(tn.LocationA),
                            LocationB = _mapper.Map<AddressModel>(tn.LocationB),
                            PickUpArrivalTime = $"{tn.PickUpArrivalDate?.ToString("dd.MM.yyyy")}",
                            PickUpDepartureTime = $"{tn.PickUpDepartureDate?.ToString("dd.MM.yyyy")}",
                            DropOffArrivalTime = $"{tn.DropOffArrivalDate?.ToString("dd.MM.yyyy")}",
                            DropOffDepartureTime = $"{tn.DropOffDepartureDate?.ToString("dd.MM.yyyy")}",
                            Transporter = tn.Transporter,

                        };
                    }
                }
                else
                {
                    tn = _dbContext.TNs
                        .Include(t => t.Material)
                        .Include(tn => tn.LocationA)
                        .Include(tn => tn.LocationB)
                        .FirstOrDefault(t => t.DriverTaskId == driverTaskId);

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
                        var driverInfo = tn.DriverTask.Driver.FullName;

                        var gp = _dbContext.Clients.FirstOrDefault(t => t.Id == tn.GpId);
                        var go = _dbContext.Clients.FirstOrDefault(t => t.Id == tn.GoId);

                        result = new EditTNModel
                        {
                            IsOriginalReceived = tn.IsOrginalReceived ?? false,
                            Go = new ClientModel
                            {
                                ClientName = go?.ClientName,
                                ClientAddress = go?.ClientAddress,
                                Id = go?.Id,
                                Inn = go?.Inn
                            },
                            Gp = new ClientModel
                            {
                                ClientName = gp?.ClientName,
                                ClientAddress = gp?.ClientAddress,
                                Id = gp?.Id,
                                Inn = gp?.Inn
                            },
                            Date = tn.DriverTask.StartDate,
                            Number = tn.Number,
                            Unit = UnitToString(tn.Unit),
                            Unit2 = UnitToString(tn.Unit2),
                            UnloadUnit = UnitToString(tn.UnloadUnit),
                            UnloadUnit2 = UnitToString(tn.UnloadUnit2),
                            LoadVolume = tn.LoadVolume?.ToString(),
                            LoadVolume2 = tn.LoadVolume2?.ToString(),
                            UnloadVolume = tn.UnloadVolume?.ToString(),
                            UnloadVolume2 = tn.UnloadVolume2?.ToString(),
                            Material = new MaterialModel
                            {
                                Id = tn.Material.Id,
                                Name = tn.Material.Name
                            },
                            MaterialAmount = $"{tn.LoadVolume} {UnitToString(tn.Unit)}",
                            LocationA = _mapper.Map<AddressModel>(tn.LocationA),
                            LocationB = _mapper.Map<AddressModel>(tn.LocationB),
                            PickUpArrivalTime = $"{tn.PickUpArrivalDate?.ToString("dd.MM.yyyy")}",
                            PickUpDepartureTime = $"{tn.PickUpDepartureDate?.ToString("dd.MM.yyyy")}",
                            DropOffArrivalTime = $"{tn.DropOffArrivalDate?.ToString("dd.MM.yyyy")}",
                            DropOffDepartureTime = $"{tn.DropOffDepartureDate?.ToString("dd.MM.yyyy")}",
                            Transporter = tn.Transporter
                        };
                    }
                }
                return result;

            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка получения: {ex.Message}", ex.Message);
            }

            return new EditTNModel();
        }

        public TNModel GetTnModel(long driverTaskId, bool isSubtask = false)
        {
            var result = new TNModel();
            TN tn;
            try
            {
                if (isSubtask)
                {
                    tn = _dbContext.TNs
                        .Include(t => t.Material)
                        .Include(t => t.Order)
                        .Include(t => t.SubTask)
                        .ThenInclude(t => t.Notes)
                        .FirstOrDefault(t => t.SubTaskId == driverTaskId);

                    if (tn != null)
                    {
                        var locationA = _dbContext.Addresses.FirstOrDefault(t => t.Id == tn.LocationAId);
                        var locationB = _dbContext.Addresses.FirstOrDefault(t => t.Id == tn.LocationBId);

                        //var driverInfo = tn.DriverTask.Driver.FullName;

                        var gp = _dbContext.Clients.FirstOrDefault(t => t.Id == tn.GpId);
                        var go = _dbContext.Clients.FirstOrDefault(t => t.Id == tn.GoId);

                        var gpInfo = $"{gp?.ClientName}, ИНН {gp?.Inn}";
                        var goInfo = $"{go?.ClientName}, ИНН {go?.Inn}";

                        var linksList = new List<string>();

                        if (tn.SubTask != null && tn.SubTask.Notes != null)
                        {
                            foreach (var note in tn.SubTask.Notes)
                            {
                                var links = JsonConvert.DeserializeObject<List<string>>(note.S3Links);
                                linksList.AddRange(links);
                            }
                        }

                        //TODO: грузоотправитель
                        result = new TNModel
                        {
                            IsOriginalReceived = tn.IsOrginalReceived ?? false,
                            IsVerified = tn.IsVerified ?? false,
                            GoInfo = goInfo,
                            Go = new ClientModel
                            {
                                ClientName = go?.ClientName,
                                ClientAddress = go?.ClientAddress,
                                Id = go.Id,
                                Inn = go?.Inn,
                                ClientUnit = go.ClientUnit
                            },
                            Gp = new ClientModel
                            {
                                ClientName = gp?.ClientName,
                                ClientAddress = gp?.ClientAddress,
                                Id = gp.Id,
                                Inn = gp?.Inn,
                                ClientUnit = go.ClientUnit
                            },
                            //DriverInfo = driverInfo,
                            //Date = tn.DriverTask.StartDate,
                            Number = tn.Number,
                            GpInfo = gpInfo,
                            Unit = UnitToString(tn.Unit),
                            Unit2 = UnitToString(tn.Unit2),
                            UnloadUnit = UnitToString(tn.UnloadUnit),
                            UnloadUnit2 = UnitToString(tn.UnloadUnit2),
                            LoadVolume = tn.LoadVolume.ToString(),
                            LoadVolume2 = tn.LoadVolume2?.ToString(),
                            UnloadVolume = tn.UnloadVolume?.ToString(),
                            UnloadVolume2 = tn.UnloadVolume2?.ToString(),
                            Material = tn.Material?.Name,
                            MaterialAmount = $"{tn.LoadVolume} {UnitToString(tn.Unit)}",
                            Transporter = tn.Transporter,
                            //CarModel = $"{tn.DriverTask.Car.Brand} {tn.DriverTask.Car.Model}",
                            //CarPlate = tn.DriverTask.Car.Plate,
                            //TrailerPlate = tn.DriverTask.Car?.Trailer?.Plate,
                            LocationA = locationA?.TextAddress,
                            LocationB = locationB?.TextAddress,
                            PickUpArrivalTime = $"{tn.PickUpArrivalDate?.ToString("dd.MM.yyyy")}",
                            PickUpDepartureTime = $"{tn.PickUpDepartureDate?.ToString("dd.MM.yyyy")}",
                            DropOffArrivalTime = $"{tn.DropOffArrivalDate?.ToString("dd.MM.yyyy")}",
                            DropOffDepartureTime = $"{tn.DropOffDepartureDate?.ToString("dd.MM.yyyy")}",
                            S3Links = linksList,
                            TransporterId = tn.TransporterId ?? 0
                        };
                    }
                }
                else
                {
                    tn = _dbContext.TNs.Include(t => t.Material).FirstOrDefault(t => t.DriverTaskId == driverTaskId);

                    var task = _dbContext.DriverTasks
                        .Include(t => t.Order)
                        .ThenInclude(t => t.Material)
                        .Include(t => t.Order)
                        .ThenInclude(t => t.Client)
                        .Include(t => t.Car)
                        .ThenInclude(c => c.Trailer)
                        .Include(dt => dt.Driver)
                        .Include(dt => dt.Notes)
                        .FirstOrDefault(t => t.Id == driverTaskId);

                    var linksList = new List<string>();

                    if (task != null)
                    {
                        foreach (var note in task.Notes)
                        {
                            var links = JsonConvert.DeserializeObject<List<string>>(note.S3Links);
                            linksList.AddRange(links);
                        }
                    }

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
                        result = new TNModel
                        {
                            IsOriginalReceived = tn.IsOrginalReceived ?? false,
                            GoInfo = goInfo,
                            Go = new ClientModel
                            {
                                ClientName = go?.ClientName,
                                ClientAddress = go?.ClientAddress,
                                Id = go.Id,
                                Inn = go?.Inn
                            },
                            Gp = new ClientModel
                            {
                                ClientName = gp?.ClientName,
                                ClientAddress = gp?.ClientAddress,
                                Id = gp.Id,
                                Inn = gp?.Inn
                            },
                            DriverInfo = driverInfo,
                            Date = tn.DriverTask.StartDate,
                            Number = tn.Number,
                            GpInfo = gpInfo,
                            Unit = UnitToString(tn.Unit),
                            Unit2 = UnitToString(tn.Unit2),
                            UnloadUnit = UnitToString(tn.UnloadUnit),
                            UnloadUnit2 = UnitToString(tn.UnloadUnit2),
                            LoadVolume = tn.LoadVolume.ToString(),
                            LoadVolume2 = tn.LoadVolume2?.ToString(),
                            UnloadVolume = tn.UnloadVolume?.ToString(),
                            UnloadVolume2 = tn.UnloadVolume2?.ToString(),
                            Material = tn.Material?.Name,
                            MaterialAmount = $"{tn.LoadVolume} {UnitToString(tn.Unit)}",
                            CarModel = $"{tn.DriverTask.Car.Brand} {tn.DriverTask.Car.Model}",
                            CarPlate = tn.DriverTask.Car.Plate,
                            TrailerPlate = tn.DriverTask.Car?.Trailer?.Plate,
                            LocationA = locationA?.TextAddress,
                            LocationB = locationB?.TextAddress,
                            PickUpArrivalTime = $"{tn.PickUpArrivalDate?.ToString("dd.MM.yyyy")}",
                            PickUpDepartureTime = $"{tn.PickUpDepartureDate?.ToString("dd.MM.yyyy")}",
                            DropOffArrivalTime = $"{tn.DropOffArrivalDate?.ToString("dd.MM.yyyy")}",
                            DropOffDepartureTime = $"{tn.DropOffDepartureDate?.ToString("dd.MM.yyyy")}",
                            S3Links = linksList,
                            Transporter = tn?.Transporter
                        };
                    }

                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка получения: {ex.Message}", ex.Message);
                return new TNModel();
            }
        }

        private string UnitToString(Unit? unit)
        {
            switch (unit)
            {
                case Unit.t:
                    return "тн";
                case Unit.m3:
                    return "m3";
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
                        Unit = Unit.m3,
                        LoadVolume2 = model.LoadVolume2,
                        Unit2 = Unit.t,
                        LocationAId = model.LocationAId,
                        LocationBId = model.LocationBId,
                        PickUpArrivalDate = model.PickUpArrivalDate?.Date,
                        PickUpArrivalTime = model.PickUpArrivalTime,
                        PickUpDepartureDate = model.PickUpDepartureDate?.Date,
                        PickUpDepartureTime = model.PickUpDepartureTime,
                        DriverTaskId = task.Id,
                        DriverId = task.DriverId,
                        MaterialId = model.MaterialId,
                        Transporter = model.Transporter,
                        TransporterId = model.TransporterId,
                        OrderId = task.OrderId
                    };

                    if (model.IsSubtask)
                    {
                        var sbTN = _dbContext.TNs.FirstOrDefault(t => t.SubTaskId == model.SubTaskId);

                        if (sbTN != null)
                        {
                            _dbContext.Update(sbTN).CurrentValues.SetValues(new
                            {
                                Number = model.Number,
                                GoId = model.GoId,
                                GpId = model.GpId,
                                LoadVolume = model.LoadVolume,
                                SubTaskId = model.SubTaskId,
                                LocationAId = model.LocationAId,
                                LocationBId = model.LocationBId,
                                PickUpArrivalDate = model.PickUpArrivalDate?.Date,
                                PickUpArrivalTime = model.PickUpArrivalTime,
                                PickUpDepartureDate = model.PickUpDepartureDate?.Date,
                                PickUpDepartureTime = model.PickUpDepartureTime,
                                DriverId = task.DriverId,
                                MaterialId = model.MaterialId,
                                Transporter = model.Transporter,
                                TransporterId = model.TransporterId,
                                OrderId = task.OrderId
                            });
                        }
                        else
                        {
                            Tn.SubTaskId = model.SubTaskId;
                            Tn.DriverTaskId = null;

                            _dbContext.TNs.Add(Tn);
                        }
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
                            _dbContext.Update(updTn).CurrentValues.SetValues(new
                            {
                                Number = model.Number,
                                GoId = model.GoId,
                                GpId = model.GpId,
                                LoadVolume = model.LoadVolume,
                                LocationAId = model.LocationAId,
                                LocationBId = model.LocationBId,
                                PickUpArrivalDate = model.PickUpArrivalDate?.Date,
                                PickUpArrivalTime = model.PickUpArrivalTime,
                                PickUpDepartureDate = model.PickUpDepartureDate?.Date,
                                PickUpDepartureTime = model.PickUpDepartureTime,
                                DriverTaskId = task.Id,
                                DriverId = task.DriverId,
                                MaterialId = model.MaterialId,
                                Transporter = model.Transporter,
                                TransporterId = model.TransporterId,
                                OrderId = task.OrderId
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
                        TN.UnloadUnit = Unit.m3;
                        TN.UnloadVolume2 = model.UnloadVolume2;
                        TN.UnloadUnit2 = Unit.t;
                        TN.DropOffArrivalDate = model.DropOffArrivalDate?.Date;
                        TN.LocationBId = model.LocationBId;
                        TN.DropOffDepartureDate = model.DropOffDepartureDate?.Date;
                        TN.DropOffArrivalTime = model.DropOffArrivalTime;
                        TN.DropOffDepartureTime = model.DropOffDepartureTime;

                        _dbContext.Update(TN);
                        _dbContext.SaveChanges();
                    }


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
                        task.TN.UnloadUnit = Unit.m3;
                        task.TN.UnloadVolume2 = model.UnloadVolume2;
                        task.TN.UnloadUnit2 = Unit.t;
                        task.TN.DropOffArrivalDate = model.DropOffArrivalDate?.Date;
                        task.TN.LocationBId = model.LocationBId;
                        task.TN.DropOffDepartureDate = model.DropOffDepartureDate?.Date;
                        task.TN.DropOffArrivalTime = model.DropOffArrivalTime;
                        task.TN.DropOffDepartureTime = model.DropOffDepartureTime;

                        _dbContext.Update(task.TN);
                        _dbContext.SaveChanges();
                    }

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

        public ApiResponse CreateSubtaskTn(FillDocumentModel model)
        {
            try
            {
                var newTN = new TN();

                var subTask = _dbContext.SubTasks
                    .Include(t => t.DriverTask)
                    .FirstOrDefault(t => t.Id == model.SubTaskId);

                if (subTask != null)
                {
                    var TN = _dbContext.TNs.FirstOrDefault(t => t.SubTaskId == model.SubTaskId);

                    if (TN == null)
                    {
                        newTN.SubTaskId = model.SubTaskId;
                        newTN.Number = model.Number;
                        newTN.GoId = model.GoId;
                        newTN.GpId = model.GpId;
                        newTN.LoadVolume = model.LoadVolume;
                        newTN.Unit = model.Unit;
                        newTN.LoadVolume2 = model.LoadVolume2;
                        newTN.Unit2 = model.Unit2;
                        newTN.LocationAId = model.LocationAId;
                        newTN.LocationBId = model.LocationBId;
                        newTN.PickUpArrivalDate = model.PickUpArrivalDate;
                        newTN.PickUpDepartureDate = model.PickUpDepartureDate;
                        newTN.MaterialId = model.MaterialId;
                        newTN.UnloadVolume = model.UnloadVolume;
                        newTN.UnloadUnit = model.UnloadUnit;
                        newTN.UnloadVolume2 = model.UnloadVolume2;
                        newTN.UnloadUnit2 = model.UnloadUnit2;
                        newTN.DropOffArrivalDate = model.DropOffArrivalDate;
                        newTN.LocationBId = model.LocationBId;
                        newTN.DropOffDepartureDate = model.DropOffDepartureDate;
                        newTN.Transporter = model.Transporter;
                        newTN.UnloadVolume2 = model.UnloadVolume2;
                        newTN.UnloadUnit2 = model.UnloadUnit2;
                        newTN.TransporterId = model.TransporterId;
                        newTN.DriverId = subTask.DriverTask.DriverId;

                        newTN.OrderId = subTask.OrderId;
                        _dbContext.TNs.Add(newTN);
                        _dbContext.SaveChanges();


                        return new ApiResponse
                        {
                            IsSuccess = true,
                            Message = "ТН создана"
                        };
                    }
                }

                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "ТН не создана"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка заполнения ТН: {model.SubTaskId}.{ex.Message}");
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Ошибка сохранения данных"
                };
            }
        }

        public async Task<ApiResponse> UpdateTN(FillDocumentModel model)
        {
            try
            {
                if (model.IsSubtask)
                {
                    var TN = _dbContext.TNs.Include(tn => tn.SubTask).FirstOrDefault(t => t.SubTaskId == model.SubTaskId);

                    if (TN != null)
                    {
                        TN.Number = model.Number;
                        TN.GoId = model.GoId;
                        TN.GpId = model.GpId;
                        TN.LoadVolume = model.LoadVolume;
                        TN.Unit = model.Unit;
                        TN.LoadVolume2 = model.LoadVolume2;
                        TN.Unit2 = model.Unit2;
                        TN.LocationAId = model.LocationAId;
                        TN.LocationBId = model.LocationBId;
                        TN.PickUpArrivalDate = model.PickUpArrivalDate;
                        TN.PickUpDepartureDate = model.PickUpDepartureDate;
                        TN.MaterialId = model.MaterialId; 
                        TN.UnloadVolume = model.UnloadVolume;
                        TN.UnloadUnit = model.UnloadUnit;
                        TN.UnloadVolume2 = model.UnloadVolume2;
                        TN.UnloadUnit2 = model.UnloadUnit2;
                        TN.DropOffArrivalDate = model.DropOffArrivalDate;
                        TN.DropOffDepartureDate = model.DropOffDepartureDate;
                        TN.Transporter = model.Transporter;
                        TN.TransporterId = model.TransporterId;
                        _dbContext.TNs.Update(TN);
                    }
                    else
                    {
                        var newTN = new TN
                        {
                            Number = model.Number,
                            GoId = model.GoId,
                            GpId = model.GpId,
                            LoadVolume = model.LoadVolume,
                            Unit = model.Unit,
                            LoadVolume2 = model.LoadVolume2,
                            Unit2 = model.Unit2,
                            LocationAId = model.LocationAId,
                            LocationBId = model.LocationBId,
                            PickUpArrivalDate = model.PickUpArrivalDate,
                            PickUpDepartureDate = model.PickUpDepartureDate,
                            MaterialId = model.MaterialId,
                            UnloadVolume = model.UnloadVolume,
                            UnloadUnit = model.UnloadUnit,
                            UnloadVolume2 = model.UnloadVolume2,
                            UnloadUnit2 = model.UnloadUnit2,
                            DropOffArrivalDate = model.DropOffArrivalDate,
                            DropOffDepartureDate = model.DropOffDepartureDate,
                            Transporter = model.Transporter,
                            TransporterId = model.TransporterId 
                        };

                        _dbContext.TNs.Add(newTN);
                    }

                    if (model.Files != null && model.Files.Count > 0)
                    {
                        var subTask = TN.SubTask;

                        if (subTask != null)
                        {
                            var taskNote = new DriverTaskNote
                            {
                                DriverTaskId = null,
                                SubTaskId = model.SubTaskId,
                                Status = DriverTaskStatus.DocumentSigning2,
                                Text = " ",
                                DateCreated = DateTime.UtcNow,
                            };

                            var links = new List<string>();

                            foreach (var file in model.Files)
                            {
                                var path = subTask.OrderId + "/" + subTask.Id + "/" + subTask.Status.ToString();

                                links.Add("cartek/" + path + "/" + file.FileName);

                                await _AWSS3Service.UploadFileToS3(file, path, file.FileName, "cartek");
                            }

                            var stringLinks = JsonConvert.SerializeObject(links);

                            taskNote.S3Links = stringLinks;

                            _dbContext.DriverTaskNotes.Add(taskNote);
                        }
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
                        task.TN.Number = model.Number;
                        task.TN.GoId = model.GoId;
                        task.TN.GpId = model.GpId;
                        task.TN.LoadVolume = model.LoadVolume;
                        task.TN.LoadVolume2 = model.LoadVolume2;
                        task.TN.Unit = model.Unit;
                        task.TN.Unit2 = model.Unit2;
                        task.TN.LocationAId = model.LocationAId; 
                        task.TN.LocationBId = model.LocationBId;
                        task.TN.PickUpArrivalDate = model.PickUpArrivalDate?.Date;
                        task.TN.PickUpDepartureDate = model.PickUpDepartureDate?.Date;
                        task.TN.MaterialId = model.MaterialId;
                        task.TN.UnloadVolume = model.UnloadVolume;
                        task.TN.UnloadUnit = model.UnloadUnit;
                        task.TN.UnloadVolume2 = model.UnloadVolume2;
                        task.TN.UnloadUnit2 = model.UnloadUnit2;
                        task.TN.DropOffArrivalDate = model.DropOffArrivalDate?.Date;
                        task.TN.LocationBId = model.LocationBId;
                        task.TN.DropOffDepartureDate = model.DropOffDepartureDate?.Date;
                        task.TN.DropOffArrivalTime = model.DropOffArrivalTime;
                        task.TN.DropOffDepartureTime = model.DropOffDepartureTime;
                        task.TN.Transporter = model.Transporter;
                        task.TN.TransporterId = model.TransporterId;
                        _dbContext.TNs.Update(task.TN);
                    }
                    else
                    {
                        var newTN = new TN
                        {
                            Number = model.Number,
                            GoId = model.GoId,
                            GpId = model.GpId,
                            LoadVolume = model.LoadVolume,
                            Unit = model.Unit,
                            LoadVolume2 = model.LoadVolume2,
                            Unit2 = model.Unit2,
                            LocationAId = model.LocationAId,
                            LocationBId = model.LocationBId,
                            PickUpArrivalDate = model.PickUpArrivalDate,
                            PickUpDepartureDate = model.PickUpDepartureDate,
                            MaterialId = model.MaterialId,
                            UnloadVolume = model.UnloadVolume,
                            UnloadUnit = model.UnloadUnit,
                            UnloadVolume2 = model.UnloadVolume2,
                            UnloadUnit2 = model.UnloadUnit2,
                            DropOffArrivalDate = model.DropOffArrivalDate,
                            DropOffDepartureDate = model.DropOffDepartureDate,
                            Transporter = model.Transporter,
                            TransporterId = model.TransporterId
                        };

                        _dbContext.TNs.Add(newTN);
                    }

                    if (model.Files != null && model.Files.Count > 0)
                    {
                        var taskNote = new DriverTaskNote
                        {
                            DriverTaskId = task.Id,
                            Status = DriverTaskStatus.DocumentSigning2,
                            Text = "Фото",
                            DateCreated = DateTime.UtcNow,
                        };

                        var links = new List<string>();


                        foreach (var file in model.Files)
                        {
                            var path = task.OrderId + "/" + task.Id + "/" + task.Status.ToString();

                            links.Add("cartek/" + path + "/" + file.FileName);

                            await _AWSS3Service.UploadFileToS3(file, path, file.FileName, "cartek");
                        }

                        var stringLinks = JsonConvert.SerializeObject(links);

                        taskNote.S3Links = stringLinks;

                        _dbContext.DriverTaskNotes.Add(taskNote);
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
                    Status = DriverTaskStatus.Unloading,
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
                    Message = subTask.Id.ToString()
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
            try
            {
                var task = _dbContext.DriverTasks.FirstOrDefault(t => t.Id == taskId);

                if (task != null)
                {
                    var taskNote = new DriverTaskNote
                    {
                        DriverTaskId = taskId,
                        Status = task.Status,
                        Text = string.IsNullOrEmpty(comment) ? " " : comment,
                        DateCreated = DateTime.UtcNow,
                        CreatedByDriver = true
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
            catch (Exception ex)
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

                    if (status >= 8)
                    {
                        task.Status = DriverTaskStatus.Done;
                    }
                    else
                    {
                        task.Status = (DriverTaskStatus)status;
                    }

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

        public ApiResponse TaskGetBack(long taskId, bool isSubtask = false)
        {
            try
            {
                if (!isSubtask)
                {
                    var task = _dbContext.DriverTasks.FirstOrDefault(t => t.Id == taskId);

                    if (task != null)
                    {
                        if (task.Status != DriverTaskStatus.Assigned)
                        {
                            var enInt = ((int)task.Status) - 1;
                            task.Status = (DriverTaskStatus)enInt;

                            _dbContext.Update(task);

                            _dbContext.SaveChanges();
                        }

                        return new ApiResponse
                        {
                            IsSuccess = true,
                            Message = "Статус обновлен"
                        };
                    }
                }
                else
                {
                    var task = _dbContext.SubTasks.FirstOrDefault(t => t.Id == taskId);

                    if (task != null)
                    {
                        if (task.Status != DriverTaskStatus.Assigned)
                        {
                            var enInt = ((int)task.Status) - 1;
                            task.Status = (DriverTaskStatus)enInt;

                            _dbContext.Update(task);

                            _dbContext.SaveChanges();
                        }

                        return new ApiResponse
                        {
                            IsSuccess = true,
                            Message = "Статус обновлен"
                        };
                    }
                }

                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Статус не обновлен"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Невозможно обновить статус.{ex.Message},Trace:{ex.StackTrace}");
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Статус не обновлен"
                };
            }
        }

        public ApiResponse VerifyTn(long driverTaskId, bool isOriginalReceived, bool isSubTask = false)
        {
            var response = new ApiResponse
            {
                IsSuccess = false,
                Message = "ТН подтверждена"
            };

            try
            {
                if (isSubTask)
                {
                    var tn = _dbContext.SubTasks.Include(t => t.TN).FirstOrDefault(tn => tn.Id == driverTaskId)?.TN;
                    if (tn != null)
                    {
                        tn.IsVerified = true;
                        tn.IsOrginalReceived = isOriginalReceived;

                        _dbContext.Update(tn);

                        _dbContext.SaveChanges();

                        response.IsSuccess = true;
                    }
                }
                else
                {
                    var tn = _dbContext.TNs.FirstOrDefault(tn => tn.DriverTaskId == driverTaskId);
                    if (tn != null)
                    {
                        tn.IsVerified = true;
                        tn.IsOrginalReceived = isOriginalReceived;

                        _dbContext.Update(tn);
                        _dbContext.SaveChanges();

                        response.IsSuccess = true;
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка подтверждения ТН. {ex.Message}", ex.Message);
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Ошибка подтверждения ТН"
                };
            }
        }

        public SubTaskModel GetSubTask(long subTaskId)
        {
            var subTask = _dbContext.SubTasks.SingleOrDefault(t => t.Id == subTaskId);
            var result = new SubTaskModel();

            if (subTask != null)
            {
                var driverTask = GetDriverTaskById(subTask.DriverTaskId);
                result = _mapper.Map<SubTaskModel>(subTask);

                var dt = _mapper.Map<DriverTaskSubTaskModel>(driverTask);

                if (driverTask != null)
                {
                    foreach(var task in dt.Order.DriverTasks)
                    {
                        task.SubTasks = null;
                    }
                    
                    DriverTaskExportModelSetLocations(dt);
                    result.DriverTask = dt;
                }
            }

            return result;
        }

        public async Task<ApiResponse> DeleteImage(DeleteImageRequest request)
        {
            try
            {
                var note = _dbContext.DriverTaskNotes.FirstOrDefault(t => t.Id == request.NoteId);

                if (note != null)
                {
                    var s3Links = JsonConvert.DeserializeObject<List<string>>(note.S3Links);

                    var url = request.UrlToDelete.Substring(7);

                    await _AWSS3Service.DeleteFromS3(url);

                    s3Links.Remove(request.UrlToDelete);

                    note.S3Links = JsonConvert.SerializeObject(s3Links);

                    _dbContext.SaveChanges();

                    return new ApiResponse
                    {
                        IsSuccess = true,
                        Message = "Удалено"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка удаления S3. {ex.Message}. {ex.StackTrace}");
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Ошибка удаления"
                };
            }
            return new ApiResponse
            {
                IsSuccess = false,
                Message = "Ошибка удаления"
            };
        }

        public ApiResponse DeleteSubTask(long subTaskId)
        {
            try
            {
                var subTask = _dbContext.SubTasks.Include(st => st.Notes).FirstOrDefault(t => t.Id == subTaskId);
                var tn = _dbContext.TNs.FirstOrDefault(t => t.SubTaskId == subTaskId);

                if (subTask != null)
                {
                    var task = _dbContext.DriverTasks.FirstOrDefault(t => t.Id == subTask.DriverTaskId);

                    if (task != null)
                    {
                        if (task.SubTasksCount > 0)
                        {
                            task.SubTasksCount--;
                        }
                    }

                    if (subTask.Notes != null)
                    {
                        _dbContext.DriverTaskNotes.RemoveRange(subTask.Notes);
                    }

                    if (tn != null)
                        _dbContext.TNs.Remove(tn);

                    _dbContext.SubTasks.Remove(subTask);

                    _dbContext.SaveChanges();

                    return new ApiResponse
                    {
                        IsSuccess = true,
                        Message = "Подзадача удалена"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace, ex);
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Подзадача не удалена"
                };
            }


            return new ApiResponse
            {
                IsSuccess = false,
                Message = "Подзадача не удалена"
            };
        }

        public ApiResponse CancelDriverTask(long driverTaskId)
        {
            var driverTask = _dbContext.DriverTasks.FirstOrDefault(t => t.Id == driverTaskId);

            if (driverTask != null)
            {
                driverTask.IsCanceled = true;

                _dbContext.Update(driverTask);

                _dbContext.SaveChanges();

                return new ApiResponse { IsSuccess = true, Message = "Задача обновлена" };
            }

            return new ApiResponse { IsSuccess = false, Message = "Задача не обновлена" };
        }

        public ApiResponse RestoreDriverTask(long driverTaskId)
        {
            var driverTask = _dbContext.DriverTasks.FirstOrDefault(t => t.Id == driverTaskId);

            if (driverTask != null)
            {
                driverTask.IsCanceled = false;

                _dbContext.Update(driverTask);

                _dbContext.SaveChanges();

                return new ApiResponse { IsSuccess = true, Message = "Задача обновлена" };
            }

            return new ApiResponse { IsSuccess = false, Message = "Задача не обновлена" };
        }

        public ApiResponse CancelDriverSubTask(long driverTaskId)
        {
            var driverTask = _dbContext.SubTasks.FirstOrDefault(t => t.Id == driverTaskId);

            if (driverTask != null)
            {
                driverTask.IsCanceled = true;

                _dbContext.Update(driverTask);

                _dbContext.SaveChanges();

                return new ApiResponse { IsSuccess = true, Message = "Задача обновлена" };
            }

            return new ApiResponse { IsSuccess = false, Message = "Задача не обновлена" };
        }

        public ApiResponse RestoreDriverSubTask(long driverTaskId)
        {
            var driverTask = _dbContext.SubTasks.FirstOrDefault(t => t.Id == driverTaskId);

            if (driverTask != null)
            {
                driverTask.IsCanceled = false;

                _dbContext.Update(driverTask);

                _dbContext.SaveChanges();

                return new ApiResponse { IsSuccess = true, Message = "Задача обновлена" };
            }

            return new ApiResponse { IsSuccess = false, Message = "Задача не обновлена" };
        }
    }
}
