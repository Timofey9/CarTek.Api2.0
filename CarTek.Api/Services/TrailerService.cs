using CarTek.Api.DBContext;
using CarTek.Api.Model;
using CarTek.Api.Model.Response;
using CarTek.Api.Services.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CarTek.Api.Services
{
    public class TrailerService : ITrailerService
    {
        private readonly ApplicationDbContext _dbContext;

        private readonly ILogger<TrailerService> _logger;

        public TrailerService(ILogger<TrailerService> logger, ApplicationDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public ApiResponse CreateTrailer(CreateTrailerModel model)
        {
            var carInDb = _dbContext.Trailers.FirstOrDefault(t => t.Plate.Equals(model.Plate.ToLower()));

            if (carInDb == null)
            {
                var trailerModel = new Trailer
                {
                    Brand = model.Brand,
                    Plate = model.Plate,
                    Model = model.Model,
                    CarId = model.CarId,
                    AxelsCount = model.AxelsCount
                };

                var trailerEntity = _dbContext.Trailers.Add(trailerModel);

                var trailer = _dbContext.Trailers.FirstOrDefault(t => t.CarId == model.CarId);


                if (trailer != null)
                {
                    trailer.CarId = null;
                }

                _dbContext.SaveChanges();

                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "Полуприцеп успешно добавлен"
                };
            }

            return new ApiResponse
            {
                IsSuccess = false,
                Message = "Полуприцеп с таким гос. номером уже существует",
            };
        }

        public IEnumerable<Trailer> GetAll()
        {
            return GetAll(null, null, 0, 0, null, null);
        }

        public IEnumerable<Trailer> GetAll(string searchColumn, string search)
        {
            return GetAll(null, null, 0, 0, searchColumn, search);
        }

        public IEnumerable<Trailer> GetAll(string sortColumn, string sortDirection, int pageNumber, int pageSize, string searchColumn, string search)
        {
            pageNumber = pageNumber > 0 ? pageNumber : 1;
            pageSize = pageSize >= 0 ? pageSize : 10;

            var result = new List<Trailer>();

            try
            {
                Expression<Func<Trailer, bool>> filterBy = x => true;
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

                Expression<Func<Trailer, object>> orderBy = x => x.Id;

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

                var tresult = _dbContext.Trailers
                        .Include(x => x.Car)
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
                _logger.LogError(ex, "Не удалось получить список полуприцепов");
            }

            return result;
        }

        public Trailer GetByPlate(string plate)
        {
            var trailer = _dbContext.Trailers
                .Include(t => t.Car)
                .FirstOrDefault(trailer => trailer.Plate.ToLower().Equals(plate.ToLower()));

            if(trailer != null)
                trailer.Plate = trailer.Plate.ToUpper();

            return trailer;
        }

        public ApiResponse UpdateTrailer(long trailerId, JsonPatchDocument<Trailer> patchDoc)
        {
            try
            {
                var existing = _dbContext.Trailers.FirstOrDefault(t => t.Id == trailerId);

                if (existing == null)
                {
                    return null;
                }

                patchDoc.ApplyTo(existing);

                //Снять текущего водителя с машины
                var attachedTrailer = _dbContext.Trailers.FirstOrDefault(t => t.Id == existing.CarId);

                if (attachedTrailer != null && attachedTrailer.Id != trailerId)
                {
                    attachedTrailer.CarId = null;
                }

                _dbContext.Trailers.Update(existing);

                var modifiedEntries = _dbContext.ChangeTracker
                       .Entries()
                       .Where(x => x.State == EntityState.Added || x.State == EntityState.Modified || x.State == EntityState.Deleted || x.State == EntityState.Detached)
                       .Select(x => $"{x.DebugView.LongView}.\nState: {x.State}")
                       .ToList();

                _dbContext.SaveChanges();

                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "Полуприцеа успешно изменен"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Не удалось изменить полуприцеп {trailerId}, {ex.Message}");

                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Не удалось изменить полуприцеп"
                };
            }
        }
    }
}
