using CarTek.Api.DBContext;
using CarTek.Api.Model;
using CarTek.Api.Model.Orders;
using CarTek.Api.Model.Response;
using CarTek.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CarTek.Api.Services
{
    public class OrderService : IOrderService
    {
        private readonly ILogger<OrderService> _logger;
        private readonly ApplicationDbContext _dbContext;

        public OrderService(ILogger<OrderService> logger, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public Task<DriverTask> CreateDriverTask(CreateDriverTaskModel model)
        {
            try
            {
                var driverTask = new DriverTask
                {
                    DriverId = model.DriverId,
                    CarId = model.CarId,
                    Shift = model.Shift,
                    UniqueId = Guid.NewGuid(),
                    OrderId = model.OrderId
                };
            }
            catch
            {

            }
            throw new NotImplementedException();
        }

        public async Task<ApiResponse> CreateOrder(CreateOrderModel model)
        {
            try
            {
                var order = new Order
                {
                    Name = model.Name,
                    ClientName = model.ClientName,
                    ClientInn = model.ClientInn,
                    Volume = model.Volume,
                    LoadUnit = model.LoadUnit,
                    UnloadUnit = model.UnloadUnit,
                    IsComplete = model.IsComplete,
                    StartDate = model.StartDate.ToUniversalTime(),
                    DueDate = model.DueDate.ToUniversalTime(),
                    LocationA = model.LocationA,
                    LocationB = model.LocationB,
                    Price = model.Price,
                    Note = model.Note,
                    CarCount = model.CarCount,
                    MaterialId = model.MaterialId,
                    Service = model.Service,
                };

                var orderEntity = _dbContext.Orders.Add(order);

                _dbContext.SaveChanges();

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
                Expression<Func<Order, bool>> filterBy = x => x.StartDate.Date >= startDate.Date && x.StartDate.Date <= endDate.Date;

                if (!string.IsNullOrEmpty(searchColumn) && !string.IsNullOrEmpty(search))
                {
                    switch (searchColumn)
                    {
                        case "clientname":
                            filterBy = x => x.ClientName.ToLower().Contains(search.ToLower().Trim())
                            && x.StartDate.Date >= startDate.Date && x.StartDate.Date <= endDate.Date;
                            break;
                        case "name":
                            filterBy = x => x.Name.ToLower().Contains(search.ToLower().Trim())                            
                            && x.StartDate.Date >= startDate.Date && x.StartDate.Date <= endDate.Date;
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
    }
}
