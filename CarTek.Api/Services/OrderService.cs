using AutoMapper;
using CarTek.Api.DBContext;
using CarTek.Api.Model;
using CarTek.Api.Model.Dto;
using CarTek.Api.Model.Orders;
using CarTek.Api.Model.Response;
using CarTek.Api.Services.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using NPOI.POIFS.Properties;
using System.Globalization;
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

                if(order?.DriverTasks.Count >= order?.CarCount)
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

                    if(currentCarTask != null)
                    {
                        message = $"У машины {currentCarTask.Car.Plate} уже есть задача на указанное число, переназначить?";
                    }

                    var currentDriverTask = _dbContext.DriverTasks
                        .Include(t => t.Driver)
                        .Where(dt => dt.DriverId == model.DriverId
                    && dt.StartDate.Date == model.TaskDate.Date && dt.Shift == model.Shift)
                        .FirstOrDefault();

                    if(currentDriverTask != null)
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

                    return new ApiResponse{
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
            catch(Exception ex)
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
                    Density = model.Density
                };

                var locationA = _addressService.GetAddress(model.AddressAId);
                var locationB = _addressService.GetAddress(model.AddressBId);

                var client = _clientService.GetClient(model.ClientId);

                if(client != null)
                {
                    order.ClientId = model.ClientId;
                }

                if(locationA != null)
                {
                    order.LocationAId = locationA.Id;
                    order.LocationA = locationA.Coordinates;
                }

                if(locationB != null)
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
                        .Include(t => t.Client)
                        .Include(t => t.Material)
                        .Where(filterBy);
                    
                tresult = tresult.OrderByDescending(orderBy);               

                if (pageSize > 0)
                {
                    tresult = tresult.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                }

                foreach (var item in tresult.ToList())
                {
                    var gp = _clientService.GetClient(item.GpId);

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
                            case "address":
                                if ((!string.IsNullOrEmpty(item.LocationA) && item.LocationA.ToLower().Contains(search.ToLower())) 
                                    || (!string.IsNullOrEmpty(item.LocationB) && item.LocationB.ToLower().Contains(search.ToLower())))
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
                        .Where(filterBy);

                result = tresult.ToList();

                var filtered = new List<Order>();

                foreach (var item in result)
                {
                    var locationA = _addressService.GetAddress(item.LocationAId ?? 0);
                    var locationB = _addressService.GetAddress(item.LocationBId ?? 0);

                    if(locationA != null)
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
            catch(Exception ex)
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
                            FixedPrice = order.Client?.FixedPrice
                        },
                        Gp = new ClientModel
                        {
                            Id = gp?.Id,
                            Inn = gp?.Inn,
                            ClientName = gp?.ClientName,
                            ClientAddress = gp?.ClientAddress,
                            FixedPrice = gp?.FixedPrice
                        },
                        LocationA = locationA,
                        LocationB = locationB,
                        LoadUnit = order.LoadUnit ?? Unit.none,
                        StartDate = order.StartDate,
                        DueDate = order?.DueDate,
                        Price = order.Price,
                        MaterialPrice = order.MaterialPrice,
                        Service = order.Service,
                        CarCount = order.CarCount,
                        Density = order.Density,
                        Mileage = order.Mileage,
                        Volume = order?.Volume,
                        DriverTasks = _mapper.Map<List<DriverTaskOrderModel>>(order.DriverTasks)
                    };

                    if(order.Material != null)
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
            catch(Exception ex)
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
            catch(Exception ex)
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
                if(order != null)
                {
                    _dbContext.Orders.Remove(order);

                    foreach(var dt in order.DriverTasks)
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
            catch(Exception ex)
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
            Expression<Func<TN, bool>> filterBy;

            filterBy = x =>
                x.DriverId == driverId
                && x.PickUpDepartureDate != null && x.DropOffDepartureDate != null 
                && (x.PickUpDepartureDate.Value.Date >= date1
                && x.DropOffDepartureDate.Value.Date <= date2);


            Expression<Func<TN, object>> orderBy = x => x.PickUpDepartureDate;

            var tresult = _dbContext.TNs
                .Include(tn => tn.SubTask)
                .Include(tn => tn.DriverTask)
                    .ThenInclude(dt => dt.Car)
                .Include(tn => tn.DriverTask)
                    .ThenInclude(dt => dt.Driver)
                .Include(tn => tn.DriverTask)
                    .ThenInclude(dt => dt.Order)
                .Include(tn => tn.Material)
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
                    add = tn.SubTask.Status == DriverTaskStatus.Done || !completedOnly;

                    var parent = _dbContext.DriverTasks
                        .Include(dt => dt.Driver)
                        .Include(dt => dt.Car)
                        .Include(dt => dt.Order)
                        .FirstOrDefault(t => t.Id == tn.SubTask.DriverTaskId);

                    if (parent != null)
                    {
                        driverInfo = parent.Driver.FullName;
                        driverPercent = parent.Driver.Percentage;
                        carInfo = $"{parent.Car.Plate.ToUpper()} {parent.Car.Brand}";
                        client = parent.Order.Service == ServiceType.Supply ? gp.ClientName : go.ClientName;

                        fixedPrice = parent.Order.Service == ServiceType.Supply ? gp.FixedPrice : go.FixedPrice;

                        clientObject = parent.Order.Service == ServiceType.Supply ? gp : go;
                        order = parent.Order;
                        status = parent.Status;
                    }
                }
                else
                {
                    add = tn.DriverTask.Status == DriverTaskStatus.Done || !completedOnly;

                    carInfo = $"{tn.DriverTask.Car.Plate} {tn.DriverTask.Car.Brand}";
                    driverInfo = tn.DriverTask.Driver.FullName;
                    driverPercent = tn.DriverTask.Driver.Percentage;

                    client = tn.DriverTask.Order.Service == ServiceType.Supply ? gp.ClientName : go.ClientName;
                    fixedPrice = tn.DriverTask.Order.Service == ServiceType.Supply ? gp.FixedPrice : go.FixedPrice;

                    clientObject = tn.DriverTask.Order.Service == ServiceType.Supply ? gp : go;

                    order = tn.DriverTask.Order;

                    status = tn.DriverTask.Status;
                }

                double volume1 = tn.LoadVolume ?? 0;
                if (order.LoadUnit == Unit.m3)
                {
                    volume1 = tn.LoadVolume ?? 0;
                }
                else
                {
                    volume1 = tn.LoadVolume2 ?? 0;
                }

                double volume2 = tn.UnloadVolume ?? 0;
                if (order.LoadUnit == Unit.m3)
                {
                    volume2 = tn.UnloadVolume ?? 0;
                }
                else
                {
                    volume2 = tn.UnloadVolume2 ?? 0;
                }

                //TODO: грузоотправитель
                var model = new TNModel
                {
                    IsOriginalReceived = tn.IsOrginalReceived ?? false,
                    IsVerified = tn.IsVerified ?? false,
                    Go = new ClientModel
                    {
                        ClientName = go?.ClientName,
                        ClientAddress = go?.ClientAddress,
                        Id = go.Id,
                        Inn = go?.Inn
                    },
                    Client = client,
                    Gp = new ClientModel
                    {
                        ClientName = gp?.ClientName,
                        ClientAddress = gp?.ClientAddress,
                        Id = gp.Id,
                        Inn = gp?.Inn
                    },
                    DriverInfo = driverInfo,
                    Number = tn.Number,
                    Unit = UnitToString(clientObject?.ClientUnit),

                    UnloadUnit = UnitToString(clientObject?.ClientUnit),

                    LoadVolume = volume1.ToString(nfi),
                    UnloadVolume = volume2.ToString(nfi),

                    Material = tn.Material?.Name,
                    CarPlate = carInfo,
                    LocationA = locationA?.TextAddress,
                    LocationB = locationB?.TextAddress,
                    PickUpDepartureTime = $"{tn.PickUpDepartureDate?.ToString("dd.MM.yyyy")}",
                    DropOffDepartureTime = $"{tn.DropOffDepartureDate?.ToString("dd.MM.yyyy")}",
                    Order = order,
                    TaskStatus = status,
                    DriverPercent = driverPercent,
                    FixedPrice = fixedPrice
                };

                if (add)
                    tnList.Add(model);
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
                .Include(tn => tn.DriverTask)
                    .ThenInclude(dt => dt.Car)
                .Include(tn => tn.DriverTask)
                    .ThenInclude(dt => dt.Driver)
                .Include(tn => tn.DriverTask)
                    .ThenInclude(dt => dt.Order)
                .Include(tn => tn.Material)
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
                    add = tn.SubTask.Status == DriverTaskStatus.Done || !completedOnly;

                    var parent = _dbContext.DriverTasks
                        .Include(dt => dt.Driver)
                        .Include(dt => dt.Car)
                        .Include(dt => dt.Order)
                        .FirstOrDefault(t => t.Id == tn.SubTask.DriverTaskId);

                    if(parent != null)
                    {
                        driverInfo = parent.Driver.FullName;
                        driverPercent = parent.Driver.Percentage;
                        carInfo = $"{parent.Car.Plate.ToUpper()} {parent.Car.Brand}";

                        if(gp != null && go != null)
                        {
                            client = parent.Order.Service == ServiceType.Supply ? gp.ClientName : go.ClientName;
                            fixedPrice = parent.Order.Service == ServiceType.Supply ? gp.FixedPrice : go.FixedPrice;

                            clientObject = parent.Order.Service == ServiceType.Supply ? gp : go;
                        }

                        order = parent.Order;
                        status = parent.Status;
                    }
                }
                else
                {
                    if (tn.DriverTask != null)
                    {
                        add = tn.DriverTask.Status == DriverTaskStatus.Done || !completedOnly;

                        carInfo = $"{tn.DriverTask.Car.Plate} {tn.DriverTask.Car.Brand}";
                        driverInfo = tn.DriverTask.Driver.FullName;
                        driverPercent = tn.DriverTask.Driver.Percentage;

                        if (gp != null && go != null)
                        {
                            client = tn.DriverTask.Order.Service == ServiceType.Supply ? gp.ClientName : go.ClientName;
                            fixedPrice = tn.DriverTask.Order.Service == ServiceType.Supply ? gp.FixedPrice : go.FixedPrice;
                            clientObject = tn.DriverTask.Order.Service == ServiceType.Supply ? gp : go;
                        }

                        order = tn.DriverTask.Order;

                        status = tn.DriverTask.Status;
                    }
                }

                double volume1 = tn.LoadVolume ?? 0;
                if (order.LoadUnit == Unit.m3)
                {
                    volume1 = tn.LoadVolume ?? 0;
                }
                else
                {
                    volume1 = tn.LoadVolume2 ?? 0;
                }

                double volume2 = tn.UnloadVolume ?? 0;
                if (order.LoadUnit == Unit.m3)
                {
                    volume2 = tn.UnloadVolume ?? 0;
                }
                else
                {
                    volume2 = tn.UnloadVolume2 ?? 0;
                }
                //TODO: грузоотправитель
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
                    Unit = UnitToString(clientObject?.ClientUnit),
                    UnloadUnit = UnitToString(clientObject?.ClientUnit),
                    LoadVolume = volume1.ToString(nfi),
                    UnloadVolume = volume2.ToString(nfi),
                    Material = tn.Material?.Name,
                    CarPlate = carInfo,
                    LocationA = locationA?.TextAddress,
                    LocationB = locationB?.TextAddress,
                    PickUpDepartureTime = $"{tn.PickUpDepartureDate?.ToString("dd.MM.yyyy")}",
                    DropOffDepartureTime = $"{tn.DropOffDepartureDate?.ToString("dd.MM.yyyy")}",
                    Order = order,
                    TaskStatus = status,
                    DriverPercent = driverPercent,
                    FixedPrice = fixedPrice
                };

                if(add)
                    tnList.Add(model);
            }

            return tnList;
        }

        public IEnumerable<OrderModel> GetOrderModelsBetweenDates(string? searchColumn, string? search, DateTime startDate, DateTime endDate, bool isExport = false)
        {
            var result = new List<Order>();
            var filtered = new List<OrderModel>();

            try
            {
                var date1 = startDate.Date;
                var date2 = endDate.Date;

                Expression<Func<Order, bool>> filterBy = x =>
                         x.StartDate.AddHours(4).Date >= date1
                        && x.StartDate.AddHours(4).Date <= date2;

                Expression<Func<Order, object>> orderBy = x => x.Id;

                if (isExport)
                {
                    var tresult = _dbContext.Orders
                     .Include(o => o.DriverTasks)
                     .ThenInclude(dt => dt.TN)
                     .Include(o => o.DriverTasks)
                     .ThenInclude(dt => dt.SubTasks)
                     .Include(t => t.Material)
                     .Include(t => t.Client)
                     .Where(filterBy);

                    result = tresult.ToList();
                }
                else
                {
                    var tresult = _dbContext.Orders
                            .Include(o => o.DriverTasks)
                            .Include(t => t.Material)
                            .Include(t => t.Client)
                            .Where(filterBy);

                    tresult = tresult.OrderByDescending(orderBy);

                    result = tresult.ToList();
                };

                foreach (var item in result)
                {
                    var gp = _clientService.GetClient(item.GpId);

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
                                    result.Add(item);
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
