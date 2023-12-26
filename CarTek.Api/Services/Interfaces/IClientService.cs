using CarTek.Api.Model;
using CarTek.Api.Model.Response;

namespace CarTek.Api.Services.Interfaces
{
    public interface IClientService
    {
        ApiResponse CreateClient(string clientName, string inn, string clientAddress, Unit clientUnit, double? fixedPrice);

        ApiResponse DeleteClient(long id);

        ApiResponse UpdateClient(long? id, string? clientName, string? inn, string? clientAddress, Unit clientUnit, double? fixedPrice);

        Client GetClient(long id);

        ICollection<Client> GetClients();
    }
}
