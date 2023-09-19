using CarTek.Api.Model.Response;
using CarTek.Api.Model;
using CarTek.Api.Model.Orders;

namespace CarTek.Api.Services.Interfaces
{
    public interface IMaterialService
    {
        ApiResponse CreateMaterial(string name);

        ApiResponse DeleteMaterial(long id);

        ApiResponse UpdateMaterial(long? id, string name);

        Material GetMaterial(long id);

        ICollection<Material> GetMaterials();
    }
}
