using CarTek.Api.DBContext;
using CarTek.Api.Model;
using CarTek.Api.Model.Dto;
using CarTek.Api.Model.Orders;
using CarTek.Api.Model.Response;
using CarTek.Api.Services.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CarTek.Api.Services
{
    public class DriverService : IDriverService
    {
        private readonly ILogger<DriverService> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly IQuestionaryService _questionaryService;

        public DriverService(ILogger<DriverService> logger, ApplicationDbContext dbContext, IQuestionaryService questionaryService)
        {
            _logger = logger;
            _dbContext = dbContext;
            _questionaryService = questionaryService;
        }

        public ApiResponse CreateDriver(CreateDriverModel driver)
        {
            try
            {
                var car = _dbContext.Cars.FirstOrDefault(t => t.Id == driver.CarId);

                var driverModel = new Driver
                {
                    FirstName = driver.FirstName,
                    MiddleName = driver.MiddleName,
                    LastName = driver.LastName,
                    Phone = driver.Phone,
                    Password = driver.Password,
                    CarId = car?.Id
                };

                var driverEntity = _dbContext.Drivers.Add(driverModel);

                _dbContext.SaveChanges();

                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "Водитель создан"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Не удалось создать водителя {driver.LastName}", ex);
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = $"Не удалось создать водителя {driver.LastName}"
                };
            }
        }

        public ApiResponse DeleteDriver(long driverId)
        {
            var driver = _dbContext.Drivers.FirstOrDefault(t => t.Id == driverId);
            if (driver == null)
                return null;
            try
            {
                var questionariesAssociated = _dbContext.Questionaries.Where(t => t.DriverId == driverId).ToList();

                foreach (var questionary in questionariesAssociated)
                {
                    _questionaryService.DeleteQuestionary(questionary.UniqueId);
                }

                var result = _dbContext.Drivers.Remove(driver);

                _dbContext.SaveChanges();

                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = $"Водитель {driver.LastName} и осмотры связанные с ним успешно удалены"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Не удалось удалить водителя {driverId}");
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = $"Не удалось удалить водителя {driver.LastName} и осмотры связанные с ним"
                };
            }
        }

        public IEnumerable<Driver> GetAll()
        {
            return GetAll(null, null, 0, 0, null, null);
        }

        public IEnumerable<Driver> GetAll(string searchColumn, string search)
        {
            return GetAll(null, null, 0, 0, searchColumn, search);
        }

        public IEnumerable<Driver> GetAll(string sortColumn, string sortDirection, int pageNumber, int pageSize, string searchColumn, string search)
        {
            pageNumber = pageNumber > 0 ? pageNumber : 1;
            pageSize = pageSize >= 0 ? pageSize : 10;

            var result = new List<Driver>();

            try
            {
                Expression<Func<Driver, bool>> filterBy = x => true;
                if (!string.IsNullOrEmpty(searchColumn) && !string.IsNullOrEmpty(search))
                {
                    switch (searchColumn)
                    {
                        case "lastname":
                            filterBy = x => x.LastName.ToLower().Contains(search.ToLower().Trim());
                            break;
                        case "phone":
                            filterBy = x => x.Phone.ToLower().Contains(search.ToLower().Trim());
                            break;
                        default:
                            break;
                    }
                }

                Expression<Func<Driver, object>> orderBy = x => x.Id;

                if (sortColumn != null)
                {
                    switch (sortColumn)
                    {
                        case "lastname":
                            orderBy = x => x.LastName;
                            break;
                        case "phone":
                            orderBy = x => x.Phone;
                            break;
                        default:
                            break;
                    }
                }

                var tresult = _dbContext.Drivers
                        .Include(c => c.Car)
                        .Include(t => t.Questionaries)
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
                _logger.LogError(ex, "Не удалось получить список водителей");
            }

            return result;
        }

        //TODO: add pagination
        public IEnumerable<DriverTask> GetAllDriverTasks(long driverId)
        {
            return GetDriverTasksAll(null, null, driverId);
        }

        public Driver GetById(long driverdId)
        {
            var driver = _dbContext.Drivers
                .Include(d => d.Car)
                .FirstOrDefault(t => t.Id == driverdId);

            return driver;
        }

        public Driver GetByName(string lastname)
        {
            var driver = _dbContext.Drivers.FirstOrDefault(t => t.LastName == lastname);

            return driver;
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

        public Driver UpdateDriver(long driverId, JsonPatchDocument<Driver> driverModel)
        {
            try
            {
                var existing = GetById(driverId);

                if (existing == null)
                {
                    return null;
                }

                driverModel.ApplyTo(existing);

                ////Снять текущего водителя с машины
                //var assignedDriver = _dbContext.Drivers.FirstOrDefault(t => t.CarId == existing.CarId);

                //if (assignedDriver != null && assignedDriver.Id != driverId)
                //{
                //    assignedDriver.CarId = null;
                //}

                _dbContext.Drivers.Update(existing);

                var modifiedEntries = _dbContext.ChangeTracker
                       .Entries()
                       .Where(x => x.State == EntityState.Added || x.State == EntityState.Modified || x.State == EntityState.Deleted || x.State == EntityState.Detached)
                       .Select(x => $"{x.DebugView.LongView}.\nState: {x.State}")
                       .ToList();

                _dbContext.SaveChanges();

                return existing;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Не удалось изменить водителя {driverId}, {ex.Message}");
                return null;
            }
        }
    }
}
