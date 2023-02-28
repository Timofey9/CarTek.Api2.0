using CarTek.Api.DBContext;
using CarTek.Api.Model;
using CarTek.Api.Model.Dto;
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

        public DriverService(ILogger<DriverService> logger, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }
        public Driver CreateDriver(CreateDriverModel driver)
        {
            var driverInDb = _dbContext.Drivers.FirstOrDefault(t => t.Phone.Equals(driver.Phone));

            var car = _dbContext.Cars.FirstOrDefault(t => t.Id == driver.CarId);

            if (driverInDb != null)
            {
                var driverModel = new Driver
                {
                    FirstName = driver.FirstName,
                    MiddleName = driver.MiddleName,
                    LastName = driver.LastName,
                    Phone = driver.Phone,
                    Password = driver.Password,
                    Car = car
                };

                var driverEntity = _dbContext.Drivers.Add(driverModel);

                _dbContext.SaveChanges();

                return driverEntity.Entity;
            }

            return null;
        }


        public Driver DeleteDriver(long driverId)
        {
            var driver = _dbContext.Drivers.FirstOrDefault(t => t.Id == driverId);
            if (driver == null)
                return null;
            try
            {
                var result = _dbContext.Drivers.Remove(driver);
                _dbContext.SaveChanges();
                return result.Entity;
            }
            catch
            {
                _logger.LogError($"Не удалось удалить водителя {driverId}");
                return null;
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

                //Снять текущего водителя с машины
                var assignedDriver = _dbContext.Drivers.FirstOrDefault(t => t.CarId == existing.CarId);

                if (assignedDriver != null && assignedDriver.Id != driverId)
                {
                    assignedDriver.CarId = null;
                }

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
