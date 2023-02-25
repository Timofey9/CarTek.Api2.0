using CarTek.Api.DBContext;
using CarTek.Api.Model;
using CarTek.Api.Model.Dto.Car;
using CarTek.Api.Model.Quetionary;
using CarTek.Api.Services.Interfaces;
using Newtonsoft.Json;

namespace CarTek.Api.Services
{
    public class QuestionaryService : IQuestionaryService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IUserService _userService;
        private readonly ICarService _carService;
        private readonly ILogger<QuestionaryService> _logger;

        public QuestionaryService(ApplicationDbContext dbContext, IUserService userService, ICarService carService, ILogger<QuestionaryService> logger)
        {
            _dbContext = dbContext;
            _userService = userService; 
            _carService = carService;
            _logger = logger;
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

                foreach (var image in model.Images)
                {
                    await SaveImage(image, imagesPath);
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
