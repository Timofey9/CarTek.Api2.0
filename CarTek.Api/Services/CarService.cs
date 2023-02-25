using CarTek.Api.DBContext;
using CarTek.Api.Model;
using CarTek.Api.Model.Dto;
using CarTek.Api.Services.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CarTek.Api.Services
{
    public class CarService : ICarService
    {
        private readonly ILogger<CarService> _logger;
        private readonly ApplicationDbContext _dbContext;

        public CarService(ILogger<CarService> logger, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public Car CreateCar(CreateCarModel car)
        {
            var carInDb = _dbContext.Cars.FirstOrDefault(t => t.Plate.Equals(car.Plate.ToLower()));
            
            if(carInDb != null) { 
                var carModel = new Car
                {
                    Brand = car.Brand.ToLower(),
                    Plate = car.Plate.ToLower(),
                    Model = car.Model.ToLower(),
                };

                var carEntity = _dbContext.Cars.Add(carModel);

                _dbContext.SaveChanges();

                return carEntity.Entity;
            }

            return null;
        }

        public Car DeleteCar(long carId)
        {
            var car = _dbContext.Cars.FirstOrDefault(car => car.Id == carId);
            if (car == null)
                return null;
            try
            {
                var result = _dbContext.Cars.Remove(car);    
                _dbContext.SaveChanges();
                return result.Entity;
            }
            catch
            {
                _logger.LogError($"Не удалось удалить автомобиль {carId}");
                return null;
            }
        }

        public IEnumerable<Car> GetAll()
        {
            return GetAll(null, null, 0, 0, null, null);
        }

        public IEnumerable<Car> GetAll(string sortColumn, string sortDirection, int pageNumber, int pageSize, string searchColumn, string search)
        {
            pageNumber = pageNumber > 0 ? pageNumber : 1;
            pageSize = pageSize >= 0 ? pageSize : 10;

            var result = new List<Car>();

            try
            {
                Expression<Func<Car, bool>> filterBy = x => true;
                if (!string.IsNullOrEmpty(searchColumn) && !string.IsNullOrEmpty(search))
                {
                    switch (searchColumn)
                    {
                        case "plate":
                            filterBy = x => x.Plate.ToLower().Contains(search.ToLower().Trim());
                            break;
                        case "brand":
                            filterBy = x => x.Brand.ToLower().Contains(search.ToLower().Trim());
                            break;
                        case "model":
                            filterBy = x => x.Model.ToLower().Contains(search.ToLower().Trim());
                            break;
                        default:
                            break;
                    }
                }

                Expression<Func<Car, object>> orderBy = x => x.Id;

                if (sortColumn != null)
                {
                    switch (sortColumn)
                    {
                        case "brand":
                            orderBy = x => x.Brand;
                            break;
                        case "plate":
                            orderBy = x => x.Plate;
                            break;
                        default:
                            break;
                    }
                }

                var tresult = _dbContext.Cars
                        .Include(t => t.Trailer)
                        .Include(x => x.Driver)
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
                _logger.LogError(ex, "Не удалось получить список авто");
            }

            return result;
        }

        public IEnumerable<Car> GetAll(string searchColumn, string search)
        {
            return GetAll(null, null, 0, 0, searchColumn, search);
        }

        public Car GetById(long carId)
        {
            var car = _dbContext.Cars.FirstOrDefault(t => t.Id == carId);

            return car;
        }

        public Car GetByPlate(string plate)
        {
            var car = _dbContext.Cars
                .Include(t => t.Driver)
                .Include(t => t.Trailer)
                .FirstOrDefault(car => car.Plate.ToLower().Equals(plate.ToLower()));

            return car;
        }

        public Car UpdateCar(long carId, JsonPatchDocument<Car> patchDoc)
        {
            try
            {
                var existing = GetById(carId);

                if (existing == null)
                {
                    return null;
                }


                patchDoc.ApplyTo(existing);

                _dbContext.Cars.Update(existing);

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
                _logger.LogError($"Не удалось изменить автомобиль {carId}, {ex.Message}");
                return null;
            }
        }
    }
}
