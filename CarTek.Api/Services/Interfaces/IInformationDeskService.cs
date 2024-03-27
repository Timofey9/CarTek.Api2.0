using CarTek.Api.Model;
using CarTek.Api.Model.Response;

namespace CarTek.Api.Services.Interfaces
{
    public interface IInformationDeskService
    {
        ApiResponse AddMessage(string message, InformationDeskMessageType type);

        ApiResponse DeleteMessage(long id);

        ICollection<InformationDeskMessage> GetMessages();
    }
}
