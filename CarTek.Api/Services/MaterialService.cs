using CarTek.Api.DBContext;
using CarTek.Api.Model.Orders;
using CarTek.Api.Model.Response;
using CarTek.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CarTek.Api.Services
{
    public class MaterialService : IMaterialService
    {
        private readonly ApplicationDbContext _dbContext;
        private ILogger<MaterialService> _logger;

        public MaterialService(ILogger<MaterialService> logger, ApplicationDbContext applicationDbContext)
        {
            _dbContext = applicationDbContext;
            _logger = logger;
        }

        public ApiResponse CreateMaterial(string name)
        {
            try
            {
                var material = new Material
                {
                    Name = name
                };
                
                var hasMaterial = _dbContext.Materials.Any(t => t.Name.ToLower() == material.Name.ToLower());

                if(!hasMaterial)
                {
                    _dbContext.Materials.Add(material);
                    _dbContext.SaveChanges();

                    return new ApiResponse
                    {
                        IsSuccess = true,
                        Message = "Материал добавлен"
                    };
                }

                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Такой материал уже существует"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка добавления материала: {ex.Message}");
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Ошибка добавления материала"
                };
            }

        }

        public ApiResponse DeleteMaterial(long id)
        {
            try
            {
                var material = _dbContext.Materials.Include(m => m.Orders).FirstOrDefault(t => t.Id == id);
                if (material != null)
                {
                    _dbContext.Materials.Remove(material);

                    var tns = _dbContext.TNs.Where(tn => tn.MaterialId == id);

                    _dbContext.TNs.RemoveRange(tns);

                    _dbContext.SaveChanges();

                    return new ApiResponse
                    {
                        IsSuccess = true,
                        Message = "Материал удален"
                    };
                }

                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Материал не удален"
                };
            }
            catch(Exception ex)
            {
                _logger.LogError($"Ошибка удаления материала: {ex.Message}");
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Ошибка удаления материала"
                };
            }

        }

        public Material GetMaterial(long id)
        {
            throw new NotImplementedException();
        }

        public ICollection<Material> GetMaterials()
        {
            try
            {
                var materials = _dbContext.Materials.ToList();
                return materials;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось получить список материалов");
                return null;
            }
        }

        public ApiResponse UpdateMaterial(long? id, string name)
        {
            try
            {
                var material = _dbContext.Materials.FirstOrDefault(t => t.Id == id);
                if(material != null)
                {
                    material.Name = name;

                    _dbContext.Update(material);
                    _dbContext.SaveChanges();

                    return new ApiResponse
                    {
                        IsSuccess = true,
                        Message = "Материал обновлен"
                    };
                }

                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Материал не обновлен"
                };
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Не удалось обновить материал {ex.Message}");
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Материал не обновлен"
                };
            }
            throw new NotImplementedException();
        }
    }
}
