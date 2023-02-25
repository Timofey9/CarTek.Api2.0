using CarTek.Api.DBContext;
using CarTek.Api.Model;
using CarTek.Api.Model.Dto.Car;
using CarTek.Api.Model.Quetionary;
using CarTek.Api.Model.Response;
using CarTek.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CarTek.Api.Services
{
    public class QuestionaryService : IQuestionaryService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IUserService _userService;
        private readonly ICarService _carService;
        private readonly IDriverService _driverService;
        private readonly ILogger<QuestionaryService> _logger;

        public QuestionaryService(ApplicationDbContext dbContext, IUserService userService, ICarService carService, IDriverService driverService, ILogger<QuestionaryService> logger)
        {
            _dbContext = dbContext;
            _driverService = driverService;
            _userService = userService; 
            _carService = carService;
            _logger = logger;
        }

        public async Task<ApiResponse> ApproveQuestionary(long driverId, string driverPass, Guid uniqueId)
        {
            var response = new ApiResponse();
            try
            {
                _logger.LogInformation($"Подтверждение анкеты: {uniqueId}");

                var driver = _driverService.GetById(driverId);
                if(driver.Password.Equals(driverPass))
                {
                    response.IsSuccess = true;
                    response.Message = "Пароль принят анкета отправлена";

                    var questionary = GetByUniqueId(uniqueId);

                    questionary.ApprovedAt = DateTime.UtcNow;
                    questionary.WasApproved= true;

                    await _dbContext.SaveChangesAsync();

                    return response;
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Пароль не совпадает. Попробуйте еще раз";

                    return response;
                }
            }
            catch(Exception ex)
            {
                _logger.LogError($"Ошибка подтверждения пароля: {ex.Message}");
                response.IsSuccess = false;
                response.Message = $"Ошибка подтверждения пароля: {ex.Message}";

                return response;
            }
        }

        public async Task<Questionary> CreateQuestionary(CreateQuestionaryModel model)
        {
            try
            {
                var uniqueId = Guid.NewGuid();

                var carQuestionary = JsonConvert.DeserializeObject<CarQuestionaryModel>(model.CarQuestionaryModel);
                var trailerQuestionary = JsonConvert.DeserializeObject<TrailerQuestionaryModel>(model.TrailerQuestionaryModel);

                var car = _carService.GetById(carQuestionary.TransportId);

                var imagesPath = $"/data/uploads/{car.Plate}/{uniqueId}";

                // создаем папку для хранения файлов
                Directory.CreateDirectory(imagesPath);

                if(model.Images != null)
                {
                    foreach (var image in model.Images)
                    {
                        await SaveImage(image, imagesPath);
                    }
                }

                var user = _userService.GetByLogin(model.CreatedBy);

                var driver = _dbContext.Drivers.FirstOrDefault(t => t.Id == model.DriverId);

                var timeCreated = DateTime.UtcNow;

                var carQuestionaryEntity = new Questionary
                {
                    UniqueId = uniqueId,
                    ImagesPath = imagesPath,
                    Mileage = carQuestionary.Mileage,
                    UpdatedBy = user,
                    WheelsJsonObject = JsonConvert.SerializeObject(carQuestionary.WheelsJsonObject),
                    LightsJsonObject = JsonConvert.SerializeObject(carQuestionary.LightsJsonObject),
                    FendersJsonObject = JsonConvert.SerializeObject(new FendersJsonObject
                    {
                        MountState = carQuestionary.FendersMountState,
                        FendersOk = !carQuestionary.FendersOk
                    }),
                    Comment = model.Comment,
                    LastUpdated = timeCreated,
                    DriverId = model.DriverId,
                    UserId = user.Id,
                    TrailerId = model.TrailerId,
                    CarId = model.CarId
                };

                var trailerQuestionaryEntity = new Questionary
                {
                    UniqueId = uniqueId,
                    ImagesPath = imagesPath,
                    Mileage = 0,
                    UpdatedBy = user,
                    Comment = model.Comment,
                    WheelsJsonObject = JsonConvert.SerializeObject(carQuestionary.WheelsJsonObject),
                    LightsJsonObject = JsonConvert.SerializeObject(carQuestionary.LightsJsonObject),
                    FendersJsonObject = JsonConvert.SerializeObject(new FendersJsonObject
                    {
                        MountState = carQuestionary.FendersMountState,
                        FendersOk = !carQuestionary.FendersOk
                    }),
                    LastUpdated = timeCreated,
                    DriverId = model.DriverId,
                    UserId = user.Id,
                    CarId = model.CarId
                };

                _dbContext.Questionaries.Add(carQuestionaryEntity);
                _dbContext.Questionaries.Add(trailerQuestionaryEntity);

                await _dbContext.SaveChangesAsync();

                return carQuestionaryEntity;
            }
            catch(Exception ex)
            {
                _logger.LogError($"Ошибка сохранения опросника: {ex.Message}");
                return null;
            }
        }

        public Task<ICollection<Questionary>> GetByCarId(string id)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<Questionary>> GetByCarPlate(string plate)
        {
            throw new NotImplementedException();
        }

        public Questionary GetByUniqueId(Guid id)
        {
            try
            {
                var questionary = _dbContext.Questionaries
                    .Include(t => t.Driver)
                    .Include(t => t.Car)
                    .FirstOrDefault(t => t.UniqueId == id && t.CarId != null);

                return questionary;
            }
            catch(Exception ex)
            {
                _logger.LogError($"Ошибка получения опросника {ex.Message}", ex);
                return null;
            }
        }

        public async Task<string> SaveImage(IFormFile file, string path)
        {
            string fullPath = $"{path}/{file.FileName}";

            // сохраняем файл в папку uploads
            using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return fullPath;
        }
    }
}
