﻿using AutoMapper;
using CarTek.Api.DBContext;
using CarTek.Api.Model;
using CarTek.Api.Model.Dto;
using CarTek.Api.Model.Orders;
using CarTek.Api.Model.Response;
using CarTek.Api.Services.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using NPOI.POIFS.Properties;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace CarTek.Api.Services
{
    public class OrderService : IOrderService
    {
        private readonly ILogger<OrderService> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly IAddressService _addressService;
        private readonly IClientService _clientService;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;

        public OrderService(ILogger<OrderService> logger, ApplicationDbContext dbContext, IMapper mapper,
            IAddressService addressService, IClientService clientService, INotificationService notificationService)
        {
            _logger = logger;
            _dbContext = dbContext;
            _addressService = addressService;
            _clientService = clientService;
            _notificationService = notificationService;
            _mapper = mapper;
        }

        public async Task<ApiResponse> CreateDriverTask(CreateDriverTaskModel model)
        {
            string message = string.Empty;

            try
            {
                var order = _dbContext.Orders
                    .Include(t => t.DriverTasks)
                    .FirstOrDefault(t => t.Id == model.OrderId);

                if (order?.DriverTasks.Count >= order?.CarCount)
                {
                    return new ApiResponse
                    {
                        IsSuccess = false,
                        Message = $"Для этой заявки уже создано {order.DriverTasks.Count} задач из {order.CarCount}"
                    };
                }

                bool updateTask = true;

                if (!model.ForceChange)
                {
                    var currentCarTask = _dbContext.DriverTasks.Include(dt => dt.Car).Where(dt => dt.CarId == model.CarId
                    && dt.StartDate.Date == model.TaskDate.Date && dt.Shift == model.Shift)
                        .FirstOrDefault();

                    if (currentCarTask != null)
                    {
                        message = $"У машины {currentCarTask.Car.Plate} уже есть задача на указанное число, переназначить?";
                    }

                    var currentDriverTask = _dbContext.DriverTasks
                        .Include(t => t.Driver)
                        .Where(dt => dt.DriverId == model.DriverId
                    && dt.StartDate.Date == model.TaskDate.Date && dt.Shift == model.Shift)
                        .FirstOrDefault();

                    if (currentDriverTask != null)
                    {
                        message += $"\nУ {currentDriverTask.Driver.FullName} уже есть задача на указанное число, переназначить?";
                    }

                    updateTask = currentCarTask == null && currentDriverTask == null;
                }

                if (updateTask && model.DriverId != 0)
                {
                    var driverTask = new DriverTask
                    {
                        DriverId = model.DriverId,
                        CarId = model.CarId,
                        Shift = model.Shift,
                        UniqueId = Guid.NewGuid(),
                        OrderId = model.OrderId,
                        StartDate = model.TaskDate.ToUniversalTime(),
                        DateCreated = DateTime.UtcNow,
                        AdminComment = model.Comment,
                    };

                    _dbContext.DriverTasks.Add(driverTask);
                    _dbContext.SaveChanges();

                    await _notificationService.SendNotification("Новая задача", $"Вам назначена новая задача на {model.TaskDate.ToShortDateString()}. Подробности смотри в ЛК", model.DriverId, true);

                    message = "Задача создана";

                    return new ApiResponse
                    {
                        IsSuccess = true,
                        Message = message
                    };
                }
                else
                {
                    return new ApiResponse
                    {
                        IsSuccess = false,
                        Message = message
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка создания задачи", ex);
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Ошибка создания задачи"
                };
            }
        }

        public async Task<ApiResponse> CreateOrder(CreateOrderModel model)
        {
            try
            {
                var order = new Order
                {
                    Name = model.Name,
                    ClientName = model.ClientName,
                    Shift = model.Shift,
                    Volume = model.Volume,
                    LoadUnit = model.LoadUnit,
                    IsComplete = model.IsComplete,
                    StartDate = model.StartDate.ToUniversalTime(),
                    DueDate = model.DueDate?.ToUniversalTime(),
                    Price = model.Price ?? 0,
                    MaterialPrice = model.MaterialPrice ?? 0,
                    GpId = model.GpId,
                    Note = model.Note,
                    CarCount = model.CarCount,
                    MaterialId = model.MaterialId,
                    Service = model.Service,
                    Density = model.Density,
                    IsExternal = model.IsExternal,
                    ExternalTransporterId = model.ExternalTransporterId,
                    ExternalPrice = model.ExternalPrice,
                    DriverPrice = model.DriverPrice,
                    Discount = model.Discount,
                    ReportLoadType = model.ReportLoadType,
                    LoadTime = string.IsNullOrEmpty(model.LoadTime) ? "" : model.LoadTime
                };

                var locationA = _addressService.GetAddress(model.AddressAId);
                var locationB = _addressService.GetAddress(model.AddressBId);

                var client = _clientService.GetClient(model.ClientId);

                if (client != null)
                {
                    order.ClientId = model.ClientId;
                }

                if (locationA != null)
                {
                    order.LocationAId = locationA.Id;
                    order.LocationA = locationA.Coordinates;
                }

                if (locationB != null)
                {
                    order.LocationBId = locationB.Id;
                    order.LocationB = locationB.Coordinates;
                }

                var orderEntity = _dbContext.Orders.Add(order);

                await _dbContext.SaveChangesAsync();

                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = $"{orderEntity.Entity.Id}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Не удалось создать заказ", ex);
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = $"Не удалось создать заказ"
                };
            }
        }

        public IEnumerable<Order> GetAll(string sortColumn, string sortDirection, int pageNumber, int pageSize, string searchColumn, string search, DateTime startDate, DateTime endDate)
        {
            pageNumber = pageNumber > 0 ? pageNumber : 1;
            pageSize = pageSize >= 0 ? pageSize : 10;

            var result = new List<Order>();
            var date1 = startDate.Date;
            var date2 = endDate.Date;
            try
            {
                Expression<Func<Order, bool>> filterBy = x =>
                            x.StartDate.AddHours(4).Date >= date1
                            && x.StartDate.AddHours(4).Date <= date2;

                Expression<Func<Order, object>> orderBy = x => x.Id;

                var tresult = _dbContext.Orders
                    .Include(o => o.DriverTasks)
                    .ThenInclude(dt => dt.Car)
                    .Include(o => o.DriverTasks)
                    .ThenInclude(dt => dt.Driver)
                    .Include(o => o.DriverTasks)
                    .ThenInclude(dt => dt.Notes)
                    .Include(t => t.Client)
                    .Include(t => t.Material)
                    .Include(t => t.ExternalTransporter)
                    .Where(filterBy)
                    .Join(_dbContext.Addresses, t => t.LocationAId, a => a.Id, (odr, adr) => new { Order = odr, LocA = adr })
                    .Join(_dbContext.Addresses, t => t.Order.LocationBId, a => a.Id, (adr, odr) => new { Order = adr.Order, LocA = adr.LocA, LocB = odr })
                    .Join(_dbContext.Clients, t => t.Order.GpId, a => a.Id, (adr, gp) => new { Order = adr.Order, LocA = adr.LocA, LocB = adr.LocB, Gp = gp });

                tresult = tresult.OrderByDescending(t => t.Order.Id);

                foreach (var entry in tresult.ToList())
                {
                    var item = entry.Order;

                    item.LocationA = entry.LocA.TextAddress;

                    item.LocationB = entry.LocB.TextAddress;

                    var gp = _mapper.Map<ClientModel>(entry.Gp);

                    if (!string.IsNullOrEmpty(searchColumn) && !string.IsNullOrEmpty(search))
                    {
                        switch (searchColumn)
                        {
                            case "clientName":
                                if (item.Service == ServiceType.Supply)
                                {
                                    if (gp.ClientName.ToLower().Contains(search.ToLower().Trim()))
                                    {
                                        result.Add(item);
                                    }
                                }
                                else
                                {
                                    if (item.Client.ClientName.ToLower().Contains(search.ToLower().Trim()))
                                    {
                                        result.Add(item);
                                    }
                                }
                                break;

                            case "material":
                                if (item.Material.Name.ToLower().Contains(search.ToLower().Trim()))
                                {
                                    result.Add(item);
                                }
                                break;

                            case "service":
                                if (((int)item.Service).ToString() == search.ToLower())
                                {
                                    result.Add(item);
                                }
                                break;
                            case "addressA":
                                if ((!string.IsNullOrEmpty(item.LocationA) && item.LocationA.ToLower().Contains(search.ToLower())))
                                {
                                    result.Add(item);
                                }
                                break;
                            case "addressB":
                                if ((!string.IsNullOrEmpty(item.LocationB) && item.LocationB.ToLower().Contains(search.ToLower())))
                                {
                                    result.Add(item);
                                }
                                break;
                            default:
                                result.Add(item);
                                break;
                        }
                    }
                    else
                    {
                        result.Add(item);
                    }
                }

                if (pageSize > 0)
                {
                    var a = result.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                    result = a.ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось получить список заявок");
            }

            return result;
        }

        public IEnumerable<Order> GetAllBetweenDates(string? searchColumn, string search, DateTime startDate, DateTime endDate)
        {
            var result = new List<Order>();

            try
            {
                var date1 = startDate.Date;
                var date2 = endDate.Date;

                Expression<Func<Order, bool>> filterBy = x =>
                        x.StartDate.AddHours(4).Date >= date1
                        && x.StartDate.AddHours(4).Date <= date2;

                Expression<Func<Order, object>> orderBy = x => x.StartDate;

                if (!string.IsNullOrEmpty(searchColumn) && !string.IsNullOrEmpty(search))
                {
                    switch (searchColumn)
                    {
                        case "clientName":
                            filterBy = x => x.ClientName.ToLower().Contains(search.ToLower().Trim())
                            && x.StartDate.AddHours(4).Date >= date1
                            && x.StartDate.AddHours(4).Date <= date2;
                            break;

                        case "material":
                            filterBy = x => x.Material.Name.ToLower().Contains(search.ToLower().Trim())
                            && x.StartDate.AddHours(4).Date >= date1
                            && x.StartDate.AddHours(4).Date <= date2;
                            break;
                        default:
                            break;
                    }
                }

                var tresult = _dbContext.Orders
                        .Include(o => o.DriverTasks)
                        .Include(t => t.Material)
                        .Include(t => t.Client)
                        .Include(t => t.ExternalTransporter)
                        .Where(filterBy);

                result = tresult.ToList();

                var filtered = new List<Order>();

                foreach (var item in result)
                {
                    var locationA = _addressService.GetAddress(item.LocationAId ?? 0);
                    var locationB = _addressService.GetAddress(item.LocationBId ?? 0);

                    if (locationA != null)
                    {
                        item.LocationA = locationA.TextAddress;
                    }

                    if (locationB != null)
                    {
                        item.LocationB = locationB.TextAddress;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось получить список заявок");
            }

            return result;
        }

        public IEnumerable<Order> GetAllActive(DateTime startDate)
        {
            var result = new List<Order>();
            try
            {
                var date = startDate.Date.AddDays(-1);
                Expression<Func<Order, bool>> filterBy = x => x.StartDate.AddHours(4).Date >= date;

                var tresult = _dbContext.Orders.Include(t => t.Client).Where(filterBy);

                result = tresult.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось получить список заявок");
            }

            return result;
        }

        public IEnumerable<Material> GetMaterials()
        {
            try
            {
                var materials = _dbContext.Materials.ToList();
                return materials;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось получить список материалов");
                return Enumerable.Empty<Material>();
            }
        }

        public Order GetOrderById(long orderId)
        {
            var order = _dbContext
                .Orders
                .Include(t => t.DriverTasks)
                .Include(t => t.Client)
                .Include(t => t.Material)
                .Include(t => t.ExternalTransporter)
                .FirstOrDefault(t => t.Id == orderId);

            var locationA = _dbContext.Addresses.FirstOrDefault(t => t.Id == order.LocationAId);
            var locationB = _dbContext.Addresses.FirstOrDefault(t => t.Id == order.LocationBId);

            return order;
        }

        public List<TNModel> GetFullTnsCollection(DateTime startDate, DateTime endDate)
        {
            var result = new List<TNModel>();

            //1 - получаем заявки
            var orders = GetOrderModelsBetweenDates(null, null, startDate, endDate);
            var mappedList = new List<OrderModel>();

            //2 - добавляем то, чего нет
            foreach (var item in orders)
            {
                var gp = _clientService.GetClient(item.GpId);

                var mappedItem = _mapper.Map<OrderModel>(item);

                mappedItem.Gp = _mapper.Map<ClientModel>(gp);

                mappedList.Add(mappedItem);
            }

            return result;
        }

        public ApiResponse AddMaterial(string name)
        {
            try
            {
                var material = new Material
                {
                    Name = name
                };

                var entity = _dbContext.Materials.Add(material);

                _dbContext.SaveChanges();

                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = $"Материал {name} добавлен"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка создания материала", ex);
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Ошибка создания материала"
                };
            }
        }

        public OrderExportModel GetOrderExportById(long orderId)
        {
            try
            {
                var order = _dbContext
                .Orders
                .Include(t => t.DriverTasks)
                .Include(t => t.Client)
                .Include(t => t.Material)
                .Include(t => t.ExternalTransporter)
                .FirstOrDefault(t => t.Id == orderId);

                if (order != null)
                {
                    var locationA = _dbContext.Addresses.FirstOrDefault(t => t.Id == order.LocationAId);
                    var locationB = _dbContext.Addresses.FirstOrDefault(t => t.Id == order.LocationBId);
                    var gp = _dbContext.Clients.FirstOrDefault(t => t.Id == order.GpId);

                    var exportModel = new OrderExportModel
                    {
                        Id = order.Id,
                        Name = order.Name,
                        Note = order.Note,
                        Shift = order.Shift,
                        Client = new ClientModel
                        {
                            Id = order.Client?.Id,
                            Inn = order.Client?.Inn,
                            ClientName = order.Client?.ClientName,
                            ClientAddress = order.Client?.ClientAddress,
                            FixedPrice = order.Client?.FixedPrice,
                            ClientUnit = order.Client.ClientUnit
                        },
                        Gp = new ClientModel
                        {
                            Id = gp?.Id,
                            Inn = gp?.Inn,
                            ClientName = gp?.ClientName,
                            ClientAddress = gp?.ClientAddress,
                            FixedPrice = gp?.FixedPrice,
                            ClientUnit = gp.ClientUnit
                        },
                        LocationA = locationA,
                        LocationB = locationB,
                        LoadUnit = order.LoadUnit ?? Unit.none,
                        StartDate = order.StartDate,
                        DueDate = order?.DueDate,
                        Price = order.Price,
                        DriverPrice = order.DriverPrice,
                        MaterialPrice = order.MaterialPrice,
                        Service = order.Service,
                        CarCount = order.CarCount,
                        Density = order.Density,
                        Mileage = order.Mileage,
                        Volume = order?.Volume,
                        DriverTasks = _mapper.Map<List<DriverTaskOrderModel>>(order.DriverTasks),
                        ReportLoadType = order.ReportLoadType,
                        LoadTime = order.LoadTime
                    };

                    if (order.IsExternal)
                    {
                        exportModel.IsExternal = true;
                        exportModel.ExternalTransporterId = order.ExternalTransporterId;
                        exportModel.ExternalTransporter = _mapper.Map<ExternalTransporterModel>(order.ExternalTransporter);
                        exportModel.ExternalPrice = order.ExternalPrice;
                        exportModel.Discount = order.Discount;
                    }

                    if (order.Material != null)
                    {
                        exportModel.Material = new MaterialModel
                        {
                            Id = order.Material.Id,
                            Name = order.Material.Name
                        };
                    }

                    return exportModel;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Не удалось получить заявку {orderId}.{ex.Message}");
                return null;
            }
        }

        public ApiResponse UpdateOrder(long orderId, JsonPatchDocument<Order> orderModel)
        {
            try
            {
                var existing = GetOrderById(orderId);

                if (existing == null)
                {
                    return null;
                }

                orderModel.ApplyTo(existing);

                _dbContext.Orders.Update(existing);

                var modifiedEntries = _dbContext.ChangeTracker
                       .Entries()
                       .Where(x => x.State == EntityState.Added || x.State == EntityState.Modified || x.State == EntityState.Deleted || x.State == EntityState.Detached)
                       .Select(x => $"{x.DebugView.LongView}.\nState: {x.State}")
                       .ToList();

                _dbContext.SaveChanges();

                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "Заявка обновлена"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка обновления заявки.{ex.Message}");

                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Ошибка обновления заявки"
                };
            }
        }

        public ApiResponse DeleteOrder(long orderId)
        {
            try
            {
                var order = _dbContext.Orders.Include(o => o.DriverTasks).ThenInclude(dt => dt.TN).FirstOrDefault(o => o.Id == orderId);
                if (order != null)
                {
                    _dbContext.Orders.Remove(order);

                    foreach (var dt in order.DriverTasks)
                    {
                        _dbContext.DriverTaskNotes.RemoveRange(_dbContext.DriverTaskNotes.Where(dn => dn.DriverTaskId == dt.Id));
                        _dbContext.SubTasks.RemoveRange(_dbContext.SubTasks.Include(st => st.TN).Include(st => st.Notes).Where(st => st.DriverTaskId == dt.Id));
                    }

                    _dbContext.SaveChanges();

                    return new ApiResponse
                    {
                        IsSuccess = true,
                        Message = "Заявка удалена"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка удаления заявки. {ex.Message}", ex);
            }

            return new ApiResponse
            {
                IsSuccess = false,
                Message = "Ошибка удаления заявки"
            };
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

        public IEnumerable<TNModel> GetTNsBetweenDatesDriver(DateTime startDate, DateTime endDate, long driverId, bool completedOnly = false)
        {
            var date1 = startDate.Date;
            var date2 = endDate.Date;

            var tnList = new List<TNModel>();
            
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ",";
            nfi.CurrencyDecimalDigits = 2;

            var addresses = _dbContext.Addresses.ToList();
            var clients = _dbContext.Clients.ToList();

            var tnQuery = _dbContext.TNs
                .Include(tn => tn.SubTask)
                    .ThenInclude(st => st.DriverTask.Driver)
                .Include(tn => tn.SubTask)
                .Include(tn => tn.DriverTask)
                    .ThenInclude(dt => dt.Car)
                .Include(tn => tn.DriverTask)
                    .ThenInclude(dt => dt.Driver)
                .Include(tn => tn.DriverTask)
                .Include(tn => tn.Material)
                .Include(tn => tn.Order)
                .Where(x =>
                    (x.DriverId == driverId || (x.SubTask != null && x.SubTask.DriverTask.DriverId == driverId)) &&
                    (x.DriverTask != null || x.SubTask.DriverTask != null) &&
                    ((x.DriverTask != null && x.DriverTask.StartDate.Date >= date1 && x.DriverTask.StartDate.Date <= date2)
                    || (x.SubTask != null && x.SubTask.DriverTask != null && x.SubTask.DriverTask.StartDate.Date >= date1 && x.SubTask.DriverTask.StartDate.Date <= date2)));

            if (completedOnly)
            {
                tnQuery = tnQuery.Where(x => x.SubTask.Status == DriverTaskStatus.Done || x.DriverTask.Status == DriverTaskStatus.Done);
            }

            var tresult = tnQuery.OrderBy(x => x.PickUpDepartureDate).AsQueryable().ToList();

            foreach (var tn in tresult)
            {
                var locationA = addresses.FirstOrDefault(t => t.Id == tn.LocationAId);
                var locationB = addresses.FirstOrDefault(t => t.Id == tn.LocationBId);

                var gp = clients.FirstOrDefault(t => t.Id == tn.GpId);
                var go = clients.FirstOrDefault(t => t.Id == tn.GoId);

                // Volume Calculation
                double volume1 = tn.LoadVolume ?? 0;
                double volume2 = tn.UnloadVolume ?? 0;

                if (tn.Order.LoadUnit != Unit.m3)
                {
                    volume1 = tn.LoadVolume2 ?? 0;
                    volume2 = tn.UnloadVolume2 ?? 0;
                }

                var model = new TNModel
                {
                    // Construct TNModel
                    IsOriginalReceived = tn.IsOrginalReceived ?? false,
                    IsVerified = tn.IsVerified ?? false,
                    Go = new ClientModel
                    {
                        ClientName = go?.ClientName,
                        ClientAddress = go?.ClientAddress,
                        Id = go?.Id,
                        Inn = go?.Inn,
                        FixedPrice = go?.FixedPrice
                    },
                    Client = tn.Order.Service == ServiceType.Supply ? gp?.ClientName : go?.ClientName,
                    Gp = new ClientModel
                    {
                        ClientName = gp?.ClientName,
                        ClientAddress = gp?.ClientAddress,
                        Id = gp?.Id,
                        Inn = gp?.Inn,
                        FixedPrice = gp?.FixedPrice
                    },
                    DriverInfo = tn.SubTask?.DriverTask?.Driver?.FullName ?? tn.DriverTask?.Driver?.FullName,
                    Transporter = tn.Transporter,
                    Number = tn.Number,
                    Unit = UnitToString(tn.Order?.LoadUnit),
                    UnloadUnit = UnitToString(tn.Order?.LoadUnit),
                    LoadVolume = volume1.ToString(nfi),
                    UnloadVolume = volume2.ToString(nfi),
                    Material = tn.Material?.Name,
                    CarPlate = tn.SubTask?.DriverTask?.Car?.Plate.ToUpper() ?? tn.DriverTask?.Car?.Plate.ToUpper(),
                    LocationA = locationA?.TextAddress,
                    LocationB = locationB?.TextAddress,
                    PickUpDepartureTime = tn.PickUpDepartureDate?.ToString("dd.MM.yyyy"),
                    DropOffDepartureTime = tn.DropOffDepartureDate?.ToString("dd.MM.yyyy"),
                    Order = _mapper.Map<OrderModel>(tn.Order),
                    TaskStatus = tn.SubTask?.Status ?? tn.DriverTask?.Status ?? DriverTaskStatus.Assigned,
                    DriverPercent = tn.SubTask?.DriverTask?.Driver?.Percentage ?? tn.DriverTask?.Driver?.Percentage ?? 0,
                    FixedPrice = tn.Order.Service == ServiceType.Supply ? gp?.FixedPrice : go?.FixedPrice
                };

                if (tn.SubTask != null && tn.SubTask.Status == DriverTaskStatus.Done)
                {
                    tnList.Add(model);
                }
                else if (tn.DriverTask != null && tn.DriverTask.Status == DriverTaskStatus.Done)
                {
                    tnList.Add(model);
                }
            }

            return tnList;
        }

        public IEnumerable<TNModel> GetTNsBetweenDates(DateTime startDate, DateTime endDate, bool completedOnly = false)
        {
            var date1 = startDate.Date;
            var date2 = endDate.Date;

            var tnList = new List<TNModel>();
            Expression<Func<TN, bool>> filterBy;

            filterBy = x =>
                x.PickUpDepartureDate != null && x.DropOffDepartureDate != null &&
                (x.PickUpDepartureDate.Value.Date >= date1
                && x.DropOffDepartureDate.Value.Date <= date2);

            Expression<Func<TN, object>> orderBy = x => x.PickUpDepartureDate;

            var tresult = _dbContext.TNs
                .Include(tn => tn.SubTask)
                    .ThenInclude(st => st.DriverTask.Driver)
                .Include(tn => tn.SubTask)
                .Include(tn => tn.DriverTask)
                    .ThenInclude(dt => dt.Car)
                .Include(tn => tn.DriverTask)
                    .ThenInclude(dt => dt.Driver)
                .Include(tn => tn.DriverTask)
                .Include(tn => tn.Material)
                .Include(tn => tn.Order)
                .Where(filterBy)
                .OrderBy(orderBy)
                .ToList();

            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ",";

            foreach (var tn in tresult)
            {
                var locationA = _dbContext.Addresses.FirstOrDefault(t => t.Id == tn.LocationAId);
                var locationB = _dbContext.Addresses.FirstOrDefault(t => t.Id == tn.LocationBId);
                string driverInfo = "";
                string carInfo = "";
                string client = "";
                double? fixedPrice = null;
                Client clientObject = null;
                double driverPercent = 0;

                bool add = false;

                Order order = new Order();
                DriverTaskStatus status = DriverTaskStatus.Assigned;

                var gp = _dbContext.Clients.FirstOrDefault(t => t.Id == tn.GpId);
                var go = _dbContext.Clients.FirstOrDefault(t => t.Id == tn.GoId);

                if (tn.SubTask != null)
                {
                    var parent = tn.SubTask;

                    add = tn.SubTask.Status == DriverTaskStatus.Done && !tn.SubTask.DriverTask.Driver.IsExternal || !completedOnly;

                    if (parent.DriverTask.Driver != null)
                    {
                        driverInfo = parent.DriverTask.Driver.FullName;
                        driverPercent = parent.DriverTask.Driver.Percentage;
                    }

                    if (parent.DriverTask.Car != null)
                    {
                        carInfo = $"{parent.DriverTask.Car.Plate.ToUpper()} {parent.DriverTask.Car.Brand}";
                    }

                    if (gp != null && go != null)
                    {
                        client = tn.Order.Service == ServiceType.Supply ? gp.ClientName : go.ClientName;
                        fixedPrice = tn.Order.Service == ServiceType.Supply ? gp.FixedPrice : go.FixedPrice;

                        clientObject = tn.Order.Service == ServiceType.Supply ? gp : go;
                    }

                    order = tn.Order;
                    status = parent.Status;
                }
                else
                {
                    if (tn.DriverTask != null)
                    {
                        add = tn.DriverTask.Status == DriverTaskStatus.Done && !tn.DriverTask.Driver.IsExternal || !completedOnly;

                        carInfo = $"{tn.DriverTask.Car.Plate} {tn.DriverTask.Car.Brand}";
                        driverInfo = tn.DriverTask.Driver.FullName;
                        driverPercent = tn.DriverTask.Driver.Percentage;

                        if (gp != null && go != null)
                        {
                            client = tn.Order.Service == ServiceType.Supply ? gp.ClientName : go.ClientName;
                            fixedPrice = tn.Order.Service == ServiceType.Supply ? gp.FixedPrice : go.FixedPrice;
                            clientObject = tn.Order.Service == ServiceType.Supply ? gp : go;
                        }

                        order = tn.Order;

                        status = tn.DriverTask.Status;
                    }
                }

                double volume1 = tn.LoadVolume ?? 0;

                if (order?.LoadUnit== Unit.m3)
                {
                    volume1 = tn.LoadVolume ?? 0;
                }
                else
                {
                    volume1 = tn.LoadVolume2 ?? 0;
                }

                double volume2 = tn.UnloadVolume ?? 0;
                if (order?.LoadUnit == Unit.m3)
                {
                    volume2 = tn.UnloadVolume ?? 0;
                }
                else
                {
                    volume2 = tn.UnloadVolume2 ?? 0;
                }

                var model = new TNModel
                {
                    IsOriginalReceived = tn.IsOrginalReceived ?? false,
                    IsVerified = tn.IsVerified ?? false,
                    Go = new ClientModel
                    {
                        ClientName = go?.ClientName,
                        ClientAddress = go?.ClientAddress,
                        Id = go?.Id,
                        Inn = go?.Inn
                    },
                    Client = client,
                    Gp = new ClientModel
                    {
                        ClientName = gp?.ClientName,
                        ClientAddress = gp?.ClientAddress,
                        Id = gp?.Id,
                        Inn = gp?.Inn
                    },
                    DriverInfo = driverInfo,
                    Transporter = tn.Transporter,
                    Number = tn.Number,
                    Unit = UnitToString(order?.LoadUnit),
                    UnloadUnit = UnitToString(order?.LoadUnit),
                    LoadVolume = volume1.ToString(nfi),
                    UnloadVolume = volume2.ToString(nfi),
                    Material = tn.Material?.Name,
                    CarPlate = carInfo,
                    LocationA = locationA?.TextAddress,
                    LocationB = locationB?.TextAddress,
                    PickUpDepartureTime = $"{tn.PickUpDepartureDate?.ToString("dd.MM.yyyy")}",
                    DropOffDepartureTime = $"{tn.DropOffDepartureDate?.ToString("dd.MM.yyyy")}",
                    Order = _mapper.Map<OrderModel>(order),
                    TaskStatus = status,
                    DriverPercent = driverPercent,
                    FixedPrice = fixedPrice
                };

                if (add)
                    tnList.Add(model);
            }

            return tnList;
        }

        public IEnumerable<OrderModel> GetOrderModelsBetweenDates(string? searchColumn, string? search, DateTime startDate, DateTime endDate, bool isExport = false)
        {
            var filtered = new List<OrderModel>();

            try
            {
                var date1 = startDate.Date;
                var date2 = endDate.Date;

                Expression<Func<Order, bool>> filterBy = x =>
                         x.StartDate.AddHours(4).Date >= date1
                        && x.StartDate.AddHours(4).Date <= date2;

                Expression<Func<Order, object>> orderBy = x => x.Id;

                var tresult = _dbContext.Orders
                    .Include(o => o.DriverTasks)
                    .ThenInclude(dt => dt.Car)
                    .Include(o => o.DriverTasks)
                    .ThenInclude(dt => dt.Driver)
                    .Include(t => t.Client)
                    .Include(t => t.Material)
                    .Include(t => t.ExternalTransporter)
                    .Where(filterBy)
                    .Join(_dbContext.Addresses, t => t.LocationAId, a => a.Id, (odr, adr) => new { Order = odr, LocA = adr })
                    .Join(_dbContext.Addresses, t => t.Order.LocationBId, a => a.Id, (adr, odr) => new { Order = adr.Order, LocA = adr.LocA, LocB = odr })
                    .Join(_dbContext.Clients, t => t.Order.GpId, a => a.Id, (adr, gp) => new { Order = adr.Order, LocA = adr.LocA, LocB = adr.LocB, Gp = gp });

                var result = tresult.ToList();

                foreach (var entry in result)
                {
                    var item = entry.Order;

                    var gp = entry.Gp;

                    item.LocationA = entry.LocA.TextAddress;

                    item.LocationB = entry.LocB.TextAddress;

                    var mappedItem = _mapper.Map<OrderModel>(item);

                    mappedItem.Gp = _mapper.Map<ClientModel>(gp);

                    if (!string.IsNullOrEmpty(searchColumn) && !string.IsNullOrEmpty(search))
                    {
                        switch (searchColumn)
                        {
                            case "clientName":
                                if (item.Service == ServiceType.Supply)
                                {
                                    if (gp.ClientName.ToLower().Contains(search.ToLower().Trim()))
                                    {
                                        filtered.Add(mappedItem);
                                    }
                                }
                                else
                                {
                                    if (item.Client.ClientName.ToLower().Contains(search.ToLower().Trim()))
                                    {
                                        filtered.Add(mappedItem);
                                    }
                                }
                                break;

                            case "material":
                                if (item.Material.Name.ToLower().Contains(search.ToLower().Trim()))
                                {
                                    filtered.Add(mappedItem);
                                }
                                break;

                            case "service":
                                if (((int)item.Service).ToString() == search.ToLower())
                                {
                                    filtered.Add(mappedItem);
                                }
                                break;
                            case "address":
                                if ((!string.IsNullOrEmpty(item.LocationA) && item.LocationA.ToLower().Contains(search.ToLower()))
                                    || (!string.IsNullOrEmpty(item.LocationB) && item.LocationB.ToLower().Contains(search.ToLower())))
                                {
                                    filtered.Add(mappedItem);
                                }
                                break;
                            default:
                                filtered.Add(mappedItem);
                                break;
                        }
                    }
                    else
                    {
                        filtered.Add(mappedItem);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось получить список заявок");
            }

            return filtered;
        }
    }
}
