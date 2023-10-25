using AutoMapper;
using CarTek.Api.DBContext;
using CarTek.Api.Model;
using CarTek.Api.Model.Dto;
using CarTek.Api.Model.Dto.Car;
using CarTek.Api.Model.Dto.Driver;
using CarTek.Api.Model.Orders;
using CarTek.Api.Model.Response;
using CarTek.Api.Services.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq.Expressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CarTek.Api.Services
{
    public class SemiNumericComparer : IComparer<string>
    {
        /// <summary>
        /// Method to determine if a string is a number
        /// </summary>
        /// <param name="value">String to test</param>
        /// <returns>True if numeric</returns>
        public static bool IsNumeric(string value)
        {
            return int.TryParse(value, out _);
        }

        /// <inheritdoc />
        public int Compare(string s1, string s2)
        {
            const int S1GreaterThanS2 = 1;
            const int S2GreaterThanS1 = -1;

            var IsNumeric1 = IsNumeric(s1);
            var IsNumeric2 = IsNumeric(s2);

            if (IsNumeric1 && IsNumeric2)
            {
                var i1 = Convert.ToInt32(s1);
                var i2 = Convert.ToInt32(s2);

                if (i1 > i2)
                {
                    return S1GreaterThanS2;
                }

                if (i1 < i2)
                {
                    return S2GreaterThanS1;
                }

                return 0;
            }

            if (IsNumeric1)
            {
                return S2GreaterThanS1;
            }

            if (IsNumeric2)
            {
                return S1GreaterThanS2;
            }

            return string.Compare(s1, s2, true, CultureInfo.InvariantCulture);
        }
    }
    public class CarService : ICarService
    {
        private readonly ILogger<CarService> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly IQuestionaryService _questionaryService;
        private readonly IMapper _mapper;
        private readonly Interfaces.IClientService _clientService;

        public CarService(ILogger<CarService> logger, ApplicationDbContext dbContext, IQuestionaryService questionaryService, IMapper mapper, Interfaces.IClientService clientService)
        {
            _logger = logger;
            _dbContext = dbContext;
            _questionaryService = questionaryService;
            _mapper = mapper;
            _clientService = clientService;
        }

        public ApiResponse CreateCar(CreateCarModel car)
        {
            var carInDb = _dbContext.Cars.FirstOrDefault(t => t.Plate.Equals(car.Plate.ToLower()));
            
            if(carInDb == null) {
                var carModel = new Car
                {
                    Brand = car.Brand,
                    Plate = car.Plate.ToLower(),
                    Model = car.Model,
                    AxelsCount = car.AxelsCount,
                };

                var carEntity = _dbContext.Cars.Add(carModel);

                _dbContext.SaveChanges();

                var trailer = _dbContext.Trailers.FirstOrDefault(t => t.Id == car.TrailerId);

                if (trailer != null)
                {
                    trailer.CarId = carEntity.Entity.Id;
                }

                _dbContext.SaveChanges();

                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "Тягач успешно добавлен"
                };
            }

            return new ApiResponse
            {
                IsSuccess = false,
                Message = "Тягач с таким гос. номером уже существует",
            };
        }

        public ApiResponse DeleteCar(long carId)
        {
            var car = _dbContext.Cars.FirstOrDefault(car => car.Id == carId);
            if (car == null)
                return null;
            try
            {
                var trailer = _dbContext.Trailers.FirstOrDefault(t => t.CarId == carId);

                var drivers = _dbContext.Drivers.Where(t => t.CarId == carId);

                foreach(var driver in drivers) {
                    driver.CarId = null;
                }

                if(trailer != null)
                    trailer.CarId = null;

                var questionariesAssociated = _dbContext.Questionaries.Where(t => t.CarId == carId).ToList();

                foreach (var questionary in questionariesAssociated)
                {
                    _questionaryService.DeleteQuestionary(questionary.UniqueId);
                }                

                var result = _dbContext.Cars.Remove(car);    
                
                _dbContext.SaveChanges();
                
                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "Автомобиль успешно удален"
                };
            }
            catch(Exception ex)
            {
                _logger.LogError($"Не удалось удалить автомобиль {carId}", ex);
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = $"Не удалось удалить автомобиль {carId}"
                };
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
                        .Include(x => x.Drivers)
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
            try
            {
                var car = _dbContext.Cars
                    .Include(q => q.Questionaries)
                    .Include(t => t.Drivers)
                    .Include(t => t.Trailer)
                    .FirstOrDefault(car => car.Plate.ToLower() == plate.ToLower());

                if (car != null)
                    car.Plate = car.Plate.ToUpper();

                return car;
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        public IEnumerable<CarDriverTaskModel> GetCarsWithTasks(DateTime date)
        {
            var result = new List<CarDriverTaskModel>();
            try
            {
                var date1 = date.Date;

                var tresult = _dbContext.Cars
                        .Include(c => c.DriverTasks.Where(dt => 
                            dt.StartDate.AddHours(4).Date == date1 && dt.Shift == ShiftType.Night
                         || dt.StartDate.AddHours(4).Date == date1.AddDays(1) && dt.Shift == ShiftType.Day
                         || dt.StartDate.AddHours(4).Date == date1 && (dt.Shift == ShiftType.Fullday || dt.Shift == ShiftType.Unlimited)
                         || dt.StartDate.AddHours(4).Date == date1.AddDays(1) && (dt.Shift == ShiftType.Fullday || dt.Shift == ShiftType.Unlimited)))
                            .ThenInclude(dt => dt.Order)                         
                            
                        .Include(c => c.DriverTasks.Where(dt =>
                            dt.StartDate.AddHours(4).Date == date1 && dt.Shift == ShiftType.Night
                         || dt.StartDate.AddHours(4).Date == date1.AddDays(1) && dt.Shift == ShiftType.Day
                         || dt.StartDate.AddHours(4).Date == date1 && (dt.Shift == ShiftType.Fullday || dt.Shift == ShiftType.Unlimited)
                         || dt.StartDate.AddHours(4).Date == date1.AddDays(1) && (dt.Shift == ShiftType.Fullday || dt.Shift == ShiftType.Unlimited)))
                        .ThenInclude(dt => dt.Driver)
                        .ToList();

                var ordered = tresult.OrderBy(t => t.Plate.Substring(1, 3), new SemiNumericComparer());

                foreach (var car in ordered)
                {
                    var a = new CarDriverTaskModel
                    {
                        Id = car.Id,
                        Model = car.Model,
                        Plate = car.Plate,
                        Brand = car.Brand,
                        DriverTasks = car.DriverTasks.Select(dt => new DriverTaskCarModel
                        {
                            Id = dt.Id,
                            Order = _mapper.Map<OrderModel>(dt.Order),
                            UniqueId = dt.UniqueId,
                            Driver = _mapper.Map<DriverInfoModel>(dt.Driver),
                            Shift = dt.Shift,
                            Status = dt.Status,
                            StartDate = dt.StartDate,
                            Volume = dt.Volume,
                            Unit = dt.Unit,
                            LocationA = _dbContext.Addresses.FirstOrDefault(t => t.Id == dt.Order.LocationAId),
                            LocationB = _dbContext.Addresses.FirstOrDefault(t => t.Id == dt.Order.LocationBId),
                        }).ToList(),
                    };

                    foreach (var driverTask in a.DriverTasks)
                    {
                        var gp = _clientService.GetClient(driverTask.Order.GpId);

                        driverTask.Order.Gp = _mapper.Map<ClientModel>(gp);
                    }

                    result.Add(a);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось получить список авто");
            }

            return result;
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

                var trailerId = Convert.ToInt64(patchDoc.Operations[4].value);

                patchDoc.Operations.RemoveAt(4);

                patchDoc.ApplyTo(existing);

                var trailer = _dbContext.Trailers.FirstOrDefault(t => t.Id == trailerId);

                var attachedTrailer = _dbContext.Trailers.FirstOrDefault(t => t.CarId == existing.Id);

                if (attachedTrailer != null && attachedTrailer.Id != carId)
                {
                    attachedTrailer.CarId = null;
                }

                existing.Trailer = trailer;

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
