﻿using AutoMapper;
using CarTek.Api.DBContext;
using CarTek.Api.Model;
using CarTek.Api.Model.Dto;
using CarTek.Api.Model.Dto.Car;
using CarTek.Api.Model.Quetionary;
using CarTek.Api.Model.Response;
using CarTek.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Linq.Expressions;

namespace CarTek.Api.Services
{
    public class UploadedFileException : Exception
    {
        public string ErrorMessage { get; set; }
    }


    public class QuestionaryService : IQuestionaryService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<QuestionaryService> _logger;
        private readonly IAWSS3Service _awss3Service;
        private readonly IMapper _mapper;

        public QuestionaryService(ApplicationDbContext dbContext, ILogger<QuestionaryService> logger, IMapper mapper, IAWSS3Service awss3Service)
        {
            _dbContext = dbContext;
            _logger = logger;
            _mapper = mapper;
            _awss3Service = awss3Service;
        }

        public async Task<ApiResponse> ApproveQuestionary(long driverId, string driverPass, Guid uniqueId, string? acceptanceComment)
        {
            var response = new ApiResponse();
            try
            {
                _logger.LogInformation($"Подтверждение анкеты: {uniqueId}");

                var driver = _dbContext.Drivers.FirstOrDefault(t => t.Id == driverId);
                if(driver != null && driver.Password.Equals(driverPass))
                {
                    response.IsSuccess = true;
                    response.Message = "Пароль принят анкета отправлена";

                    var questionary = GetByUniqueId(uniqueId);

                    var carId = questionary.CarId ?? 0;

                    if (questionary.CarId != 0)
                    {
                        var car = _dbContext.Cars.FirstOrDefault(t => t.Id == carId);

                        if (car != null && car.State == TransportState.Base)
                        {
                            car.State = TransportState.Line;
                        }
                        else
                        if (car != null && car.State == TransportState.Line)
                        {
                            car.State = TransportState.Base;
                        }
                    }

                    questionary.ApprovedAt = DateTime.UtcNow;
                    questionary.WasApproved= true;
                    questionary.AcceptanceComment = acceptanceComment;

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

        public async Task<Questionary>CreateQuestionary(CreateQuestionaryModel model)
        {
            try
            {
                var uniqueId = Guid.NewGuid();

                var carQuestionary = JsonConvert.DeserializeObject<CarQuestionaryModel>(model.CarQuestionaryModel);
                var trailerQuestionary = JsonConvert.DeserializeObject<TrailerQuestionaryModel>(model.TrailerQuestionaryModel);

                var car = _dbContext.Cars.FirstOrDefault(t => t.Id == carQuestionary.TransportId);

                string imagesPath = "";

                if (model.Images != null)
                {
                    imagesPath = $"/data/uploads/{car.Plate}/{uniqueId}";
                }

                string action = "";

                var user = _dbContext.Users.FirstOrDefault(t => t.Login == model.CreatedBy);

                var driver = _dbContext.Drivers.FirstOrDefault(t => t.Id == model.DriverId);

                var timeCreated = DateTime.UtcNow;

                if (car.State == TransportState.Base)
                {                                 
                    action = "departure";                    
                }                  
                else                 
                if (car.State == TransportState.Line)                    
                {                        
                    action = "arrival";                    
                }                

                var carQuestionaryEntity = new Questionary
                {
                    UniqueId = uniqueId,
                    ImagesPath = imagesPath,
                    Action = action,
                    Type = "car",
                    Mileage = carQuestionary.Mileage,
                    GeneralCondition = carQuestionary.GeneralCondition,
                    User = user,
                    WheelsJsonObject = JsonConvert.SerializeObject(carQuestionary.WheelsJsonObject),
                    LightsJsonObject = JsonConvert.SerializeObject(carQuestionary.LightsJsonObject),
                    FendersJsonObject = JsonConvert.SerializeObject(new FendersJsonObject
                    {
                        MountState = carQuestionary.FendersMountState,
                        FendersOk = !carQuestionary.FendersOk
                    }),
                    Comment = model.Comment,
                    Rack = carQuestionary.Rack,
                    FrontSuspension = carQuestionary.FrontSuspension,
                    BackSuspension = carQuestionary.BackSuspension,
                    IsCabinClean = carQuestionary.IsCabinClean,
                    PlatonInPlace = carQuestionary.PlatonInPlace,
                    PlatonSwitchedOn = carQuestionary.PlatonSwitchedOn,
                    CabinCushion = carQuestionary.CabinCushion,
                    HydroEq = carQuestionary.HydroEq,
                    LastUpdated = timeCreated,
                    DriverId = model.DriverId,
                    UserId = user.Id,
                    TrailerId = trailerQuestionary?.TransportId,
                    CarId = model.CarId,
                    TechnicalComment = carQuestionary.TechnicalComment
                };

                if(trailerQuestionary != null)
                {
                    var trailerQuestionaryEntity = new Questionary
                    {
                        UniqueId = uniqueId,
                        ImagesPath = imagesPath,
                        Type = "trailer",
                        Action = action,
                        GeneralCondition = trailerQuestionary.GeneralCondition,
                        Mileage = 0,
                        User = user,
                        Comment = trailerQuestionary.TrailerComment,
                        WheelsJsonObject = JsonConvert.SerializeObject(trailerQuestionary.WheelsJsonObject),
                        LightsJsonObject = JsonConvert.SerializeObject(trailerQuestionary.LightsJsonObject),
                        FendersJsonObject = JsonConvert.SerializeObject(new FendersJsonObject
                        {
                            MountState = trailerQuestionary.FendersMountState,
                            FendersOk = !trailerQuestionary.FendersOk
                        }),
                        LastUpdated = timeCreated,
                        DriverId = model.DriverId,
                        UserId = user.Id,
                        TrailerId = trailerQuestionary.TransportId,
                        TechnicalComment = trailerQuestionary.TechnicalComment
                    };
                    _dbContext.Questionaries.Add(trailerQuestionaryEntity);
                }

                _dbContext.Questionaries.Add(carQuestionaryEntity);

                await _dbContext.SaveChangesAsync();

                if (model.Images != null)
                {
                    if (model.Images.Count > 3)
                        throw new UploadedFileException() { ErrorMessage = "Максимально можно загрузить 3 изображения" };

                    // создаем папку для хранения файлов
                    Directory.CreateDirectory(imagesPath);

                    int fileNum = 1;

                    foreach (var image in model.Images)
                    {
                        await SaveImage(image, imagesPath, fileNum.ToString());
                    }
                }

                return carQuestionaryEntity;
            }
            catch(UploadedFileException ex)
            {
                throw ex;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Ошибка создания опросника: {ex.Message}");
                return null;
            }
        }

        public Task<ICollection<Questionary>> GetByCarId(string id)
        {
            throw new NotImplementedException();
        }

        public ICollection<Questionary> GetListByCarId(long carId, string sortColumn, string sortDirection, int pageNumber, int pageSize)
        {
            pageNumber = pageNumber > 0 ? pageNumber : 1;
            pageSize = pageSize >= 0 ? pageSize : 10;

            var result = new List<Questionary>();

            try
            {
                Expression<Func<Questionary, object>> orderBy = x => x.LastUpdated;

                if (sortColumn != null)
                {
                    switch (sortColumn)
                    {
                        case "date":
                            orderBy = x => x.LastUpdated;
                            break;
                        case "driver":
                            orderBy = x => x.Driver.LastName;
                            break;
                        default:
                            break;
                    }
                }

                var tresult = _dbContext.Questionaries
                    .Include(t => t.User)
                    .Include(t => t.Driver)
                    .Where(t => t.CarId == carId);

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
                _logger.LogError(ex, "Не удалось получить список опросников");
            }

            return result;
        }

        public Questionary GetByUniqueId(Guid id)
        {
            try
            {
                var questionary = _dbContext.Questionaries
                    .Include(t => t.Driver)
                    .Include(t => t.Car)
                    .Include(t => t.User)
                    .FirstOrDefault(t => t.UniqueId == id && t.Type.Equals("car"));

                return questionary;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Ошибка получения опросника {ex.Message}", ex);
                return null;
            }
        }

        public async Task<string> SaveImage(IFormFile file, string path, string fileName)
        {
            var newFileName = fileName + Path.GetExtension(file.FileName);

            string fullPath = $"{path}/{newFileName}";

            // сохраняем файл в папку uploads
            using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return fullPath;
        }

        public ICollection<Questionary> GetAll(long carId)
        {
            return GetListByCarId(carId, null, null, 0, 0);
        }

        public async Task<ICollection<ImageModel>> GetImages(Guid uniqueId)
        {
            var questionary = _dbContext.Questionaries.FirstOrDefault(t => t.UniqueId == uniqueId);

            var result = new List<ImageModel>();

            var filesPath = questionary?.ImagesPath;

            if (!string.IsNullOrEmpty(filesPath))
            {
                foreach (var file in Directory.GetFiles(filesPath))
                {
                    var bytes = await File.ReadAllBytesAsync(file);

                    var imageModel = new ImageModel
                    {
                        ImageName = Path.GetFileName(file),
                        Extension = Path.GetExtension(file).Replace(".", ""),
                        BinaryData = bytes
                    };

                    result.Add(imageModel);
                };
            }

            return result;
        }

        public UnitQuestionaryModel GetUnitByUniqueId(Guid id)
        {
            var transportModel = new UnitQuestionaryModel();

            try
            {
                var carQuestionary = _dbContext.Questionaries
                    .Include(t => t.Driver)
                    .Include(t => t.User)
                    .Include(t => t.Car)
                    .Include(t => t.Trailer)
                    .FirstOrDefault(t => t.UniqueId == id && t.Type == "car");

                var trailerQuestionary = _dbContext.Questionaries
                    .Include(t => t.Driver)
                    .Include(t => t.Car)
                    .FirstOrDefault(t => t.UniqueId == id && t.Type == "trailer");

                var trailerQuestionaryModel = new TrailerQuestionaryModel();
                var carQuestionaryModel = new CarQuestionaryModel();

                if (trailerQuestionary != null )
                {
                    // in case deserialization fails
                    try
                    {
                        trailerQuestionaryModel = new TrailerQuestionaryModel
                        {
                            TransportId = trailerQuestionary.TrailerId ?? 0,
                            TrailerComment = trailerQuestionary.Comment,
                            GeneralCondition = trailerQuestionary.GeneralCondition,
                            WheelsJsonObject = JsonConvert.DeserializeObject<WheelsJson>(trailerQuestionary.WheelsJsonObject),
                            LightsJsonObject = JsonConvert.DeserializeObject<LightsJsonObject>(trailerQuestionary.LightsJsonObject),
                            FendersMountState = JsonConvert.DeserializeObject<FendersJsonObject>(trailerQuestionary.FendersJsonObject).MountState,
                            FendersOk = !JsonConvert.DeserializeObject<FendersJsonObject>(trailerQuestionary.FendersJsonObject).FendersOk,
                            TechnicalComment = trailerQuestionary.TechnicalComment
                        };
                    }
                    catch(Exception ex)
                    {
                        _logger.LogError($"Ошибка десериализации {id}", ex);
                    }
                }

                if (carQuestionary != null)
                {
                    try
                    {
                        carQuestionaryModel = new CarQuestionaryModel
                        {
                            TransportId = (long)carQuestionary.CarId,
                            Mileage = carQuestionary.Mileage,
                            WheelsJsonObject = JsonConvert.DeserializeObject<WheelsJson>(carQuestionary.WheelsJsonObject),
                            LightsJsonObject = JsonConvert.DeserializeObject<LightsJsonObject>(carQuestionary.LightsJsonObject),
                            FendersMountState = JsonConvert.DeserializeObject<FendersJsonObject>(carQuestionary.FendersJsonObject).MountState,
                            FendersOk = !JsonConvert.DeserializeObject<FendersJsonObject>(carQuestionary.FendersJsonObject).FendersOk,
                            Rack = carQuestionary.Rack,
                            FrontSuspension = carQuestionary.FrontSuspension,
                            BackSuspension = carQuestionary.BackSuspension,
                            IsCabinClean = carQuestionary.IsCabinClean,
                            PlatonInPlace = carQuestionary.PlatonInPlace,
                            PlatonSwitchedOn = carQuestionary.PlatonSwitchedOn,
                            CabinCushion = carQuestionary.CabinCushion,
                            HydroEq = carQuestionary.HydroEq,
                            TechnicalComment = carQuestionary.TechnicalComment
                        };
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Ошибка десериализации {id}", ex);
                    }

                    transportModel = new UnitQuestionaryModel
                    {
                        UniqueId = carQuestionary.UniqueId,
                        Type = carQuestionary.Type,
                        Action = carQuestionary.Action,
                        LastUpdated = carQuestionary.LastUpdated,
                        ApprovedAt = carQuestionary.ApprovedAt,
                        Comment = carQuestionary.Comment,
                        WasApproved = carQuestionary.WasApproved,
                        Mileage = carQuestionary.Mileage,
                        User = _mapper.Map<UserModel>(carQuestionary.User),
                        Driver = _mapper.Map<DriverModel>(carQuestionary.Driver),
                        Car = _mapper.Map<CarModel>(carQuestionary.Car),
                        Trailer = _mapper.Map<TrailerModel>(carQuestionary.Trailer),
                        CarQuestionaryModel = carQuestionaryModel,
                        TrailerQuestionaryModel = trailerQuestionaryModel,
                        AcceptanceComment = carQuestionary.AcceptanceComment
                    };
                }

                return transportModel;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка получения опросника {id}", ex);
                return null;
            }
        }

        public IEnumerable<Questionary> GetAll(string searchColumn, string search)
        {
            return GetAll(null, null, 0, 0, searchColumn, search);
        }

        public IEnumerable<Questionary> GetAll(string sortColumn, string sortDirection, int pageNumber, int pageSize, string searchColumn, string search)
        {
            pageNumber = pageNumber > 0 ? pageNumber : 1;
            pageSize = pageSize >= 0 ? pageSize : 10;

            var result = new List<Questionary>();

            try
            {
                Expression<Func<Questionary, bool>> filterBy = x => x.Type.Equals("car");
                if (!string.IsNullOrEmpty(searchColumn) && !string.IsNullOrEmpty(search))
                {
                    switch (searchColumn)
                    {
                        case "plate":
                            filterBy = x => x.Type.Equals("car") && x.Car.Plate.ToLower().Contains(search.ToLower().Trim());
                            break;
                        default:
                            break;
                    }
                }

                Expression<Func<Questionary, object>> orderBy = x => x.LastUpdated;

                if (sortColumn != null)
                {
                    switch (sortColumn)
                    {
                        case "date":
                            orderBy = x => x.LastUpdated;
                            break;
                        default:
                            break;
                    }
                }

                var tresult = _dbContext.Questionaries
                    .Include(t => t.Car)
                    .Include(t => t.Driver)
                    .Include(t => t.User)
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
                _logger.LogError(ex, "Не удалось получить список анкет");
            }

            return result;
        }

        public bool DeleteQuestionaryEntity(Questionary questionary)
        {
            try
            {
                if (questionary != null)
                {
                    var imagesPath = questionary.ImagesPath;
                    if (Directory.Exists(imagesPath))
                    {
                        Directory.Delete(imagesPath, true);
                    }

                    _dbContext.Questionaries.Remove(questionary);
                }

                _dbContext.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка удаления опросника {questionary?.UniqueId}", ex);
                return false;
            }
        }
    
        public bool DeleteQuestionary(Guid uniqueId)
        {
            try
            {
                var carQuestionary = _dbContext.Questionaries
                    .Include(t => t.Driver)
                    .Include(t => t.User)
                    .Include(t => t.Car)
                    .Include(t => t.Trailer)
                    .FirstOrDefault(t => t.UniqueId == uniqueId && t.Type == "car");

                var trailerQuestionary = _dbContext.Questionaries
                    .Include(t => t.Driver)
                    .Include(t => t.Car)
                    .FirstOrDefault(t => t.UniqueId == uniqueId && t.Type == "trailer");

                if (carQuestionary != null)
                {
                    var imagesPath = carQuestionary.ImagesPath;
                    if (Directory.Exists(imagesPath))
                    {
                        Directory.Delete(imagesPath, true);
                    }

                    _dbContext.Questionaries.Remove(carQuestionary);
                }

                if (trailerQuestionary != null)
                {
                    _dbContext.Questionaries.Remove(trailerQuestionary);
                }

                _dbContext.SaveChanges();

                return true;
            }
            catch(Exception ex)
            {
                _logger.LogError($"Ошибка удаления опросника {uniqueId}", ex);
                return false;
            }
        }
    }

    public class ImageModel
    {
        public string ImageName { get; set; }
        public string Extension { get; set; }
        public byte[] BinaryData { get; set; }
    }
}
