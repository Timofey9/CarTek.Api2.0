using CarTek.Api.DBContext;
using CarTek.Api.Model;
using CarTek.Api.Model.Orders;
using CarTek.Api.Model.Response;
using CarTek.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Drawing.Printing;
using System.Linq.Expressions;

namespace CarTek.Api.Services
{
    public class OrderService : IOrderService
    {
        private readonly ILogger<OrderService> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly IAddressService _addressService;
        private readonly IClientService _clientService;
        public OrderService(ILogger<OrderService> logger, ApplicationDbContext dbContext, IAddressService addressService, IClientService clientService)
        {
            _logger = logger;
            _dbContext = dbContext;
            _addressService = addressService;
            _clientService = clientService;
        }

        public async Task<ApiResponse> CreateDriverTask(CreateDriverTaskModel model)
        {
            string message = string.Empty;

            try
            {
                bool updateTask = true;

                if (!model.ForceChange)
                {
                    var currentCarTask = _dbContext.DriverTasks.Where(dt => dt.CarId == model.CarId
                    && dt.StartDate.Date == model.TaskDate.Date && dt.Shift == model.Shift)
                        .FirstOrDefault();

                    message = "У машины уже есть задача на указанное число, переназначить?";

                    var currentDriverTask = _dbContext.DriverTasks.Where(dt => dt.DriverId == model.DriverId
                    && dt.StartDate.Date == model.TaskDate.Date && dt.Shift == model.Shift)
                        .FirstOrDefault();

                    message = "У водителя уже есть задача на указанное число, переназначить?";

                    updateTask = currentCarTask == null && currentDriverTask == null;
                }

                if (updateTask)
                {

                    var driverTask = new DriverTask
                    {
                        DriverId = model.DriverId,
                        CarId = model.CarId,
                        Shift = model.Shift,
                        UniqueId = Guid.NewGuid(),
                        OrderId = model.OrderId,
                        StartDate = model.TaskDate
                    };

                    _dbContext.Add(driverTask);

                    await _dbContext.SaveChangesAsync();

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
                    Volume = model.Volume,
                    LoadUnit = model.LoadUnit,
                    UnloadUnit = model.UnloadUnit,
                    IsComplete = model.IsComplete,
                    StartDate = model.StartDate.ToUniversalTime(),
                    DueDate = model.DueDate.ToUniversalTime(),
                    Price = model.Price ?? 0,
                    Note = model.Note,
                    CarCount = model.CarCount,
                    MaterialId = model.MaterialId,
                    Service = model.Service,                    
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
                    Message = $"Заказ #{orderEntity.Entity.Id} создан"
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

        public IEnumerable<Order> GetAll(string searchColumn, string search)
        {
            return GetAll(null, null, 0, 0, searchColumn, search);
        }

        public IEnumerable<Order> GetAll(string sortColumn, string sortDirection, int pageNumber, int pageSize, string searchColumn, string search)
        {
            pageNumber = pageNumber > 0 ? pageNumber : 1;
            pageSize = pageSize >= 0 ? pageSize : 10;

            var result = new List<Order>();

            try
            {
                Expression<Func<Order, bool>> filterBy = x => true;
                if (!string.IsNullOrEmpty(searchColumn) && !string.IsNullOrEmpty(search))
                {
                    switch (searchColumn)
                    {
                        case "clientname":
                            filterBy = x => x.ClientName.ToLower().Contains(search.ToLower().Trim());
                            break;
                        case "name":
                            filterBy = x => x.Name.ToLower().Contains(search.ToLower().Trim());
                            break;
                        default:
                            break;
                    }
                }

                Expression<Func<Order, object>> orderBy = x => x.Id;

                //if (sortColumn != null)
                //{
                //    switch (sortColumn)
                //    {
                //        case "lastname":
                //            orderBy = x => x.LastName;
                //            break;
                //        case "phone":
                //            orderBy = x => x.Phone;
                //            break;
                //        default:
                //            break;
                //    }
                //}

                var tresult = _dbContext.Orders
                        .Include(t => t.Material)
                        .Include(o => o.DriverTasks)
                        .ThenInclude(dt => dt.Driver)
                        .Where(filterBy);

                if (sortDirection == "asc")
                {
                    tresult = tresult.OrderBy(orderBy);
                }
                else
                {
                    tresult = tresult.OrderByDescending(orderBy);
                }

                if (pageSize > 0)
                {
                    tresult = tresult.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                }

                result = tresult.ToList();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось получить список заявок");
            }

            return result;
        }

        public IEnumerable<Order> GetAll(string sortColumn, string sortDirection, int pageNumber, int pageSize, string searchColumn, string search, DateTime startDate, DateTime endDate)
        {
            pageNumber = pageNumber > 0 ? pageNumber : 1;
            pageSize = pageSize >= 0 ? pageSize : 10;

            var result = new List<Order>();

            try
            {
                Expression<Func<Order, bool>> filterBy = x => x.StartDate.Date >= startDate.Date.AddDays(-1) && x.StartDate.Date <= endDate.Date;

                if (!string.IsNullOrEmpty(searchColumn) && !string.IsNullOrEmpty(search))
                {
                    switch (searchColumn)
                    {
                        case "clientname":
                            filterBy = x => x.ClientName.ToLower().Contains(search.ToLower().Trim())
                            && x.StartDate.Date >= startDate.Date.AddDays(-1) && x.StartDate.Date <= endDate.Date;
                            break;
                        case "name":
                            filterBy = x => x.Name.ToLower().Contains(search.ToLower().Trim())
                            && x.StartDate.Date >= startDate.Date.AddDays(-1) && x.StartDate.Date <= endDate.Date;
                            break;
                        default:
                            break;
                    }
                }

                Expression<Func<Order, object>> orderBy = x => x.Id;

                //if (sortColumn != null)
                //{
                //    switch (sortColumn)
                //    {
                //        case "lastname":
                //            orderBy = x => x.LastName;
                //            break;
                //        case "phone":
                //            orderBy = x => x.Phone;
                //            break;
                //        default:
                //            break;
                //    }
                //}

                var tresult = _dbContext.Orders
                        .Include(o => o.DriverTasks)
                        .ThenInclude(dt => dt.Car)
                        .Where(filterBy);

                if (sortDirection == "asc")
                {
                    tresult = tresult.OrderBy(orderBy);
                }
                else
                {
                    tresult = tresult.OrderByDescending(orderBy);
                }

                if (pageSize > 0)
                {
                    tresult = tresult.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                }

                result = tresult.ToList();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось получить список заявок");
            }

            return result;
        }

        public IEnumerable<Order> GetAllBetweenDates(DateTime startDate, DateTime endDate)
        {
            var result = new List<Order>();

            try
            {
                Expression<Func<Order, bool>> filterBy = x => x.StartDate.Date >= startDate.Date.AddDays(-1) && x.StartDate.Date <= endDate.Date;

                Expression<Func<Order, object>> orderBy = x => x.StartDate;


                var tresult = _dbContext.Orders
                        .Include(o => o.DriverTasks)
                        .Include(t => t.Material)
                        .Where(filterBy);

                result = tresult.ToList();

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
                Expression<Func<Order, bool>> filterBy = x => x.DueDate.Date >= startDate.Date.AddDays(-1);

                var tresult = _dbContext.Orders.Where(filterBy);

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
                .FirstOrDefault(t => t.Id == orderId);

            return order;
        }
    }
}
