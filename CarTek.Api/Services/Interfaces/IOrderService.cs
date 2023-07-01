using CarTek.Api.Model;
using CarTek.Api.Model.Orders;
using CarTek.Api.Model.Quetionary;
using CarTek.Api.Model.Response;

namespace CarTek.Api.Services.Interfaces
{
    public interface IOrderService
    {
        Task<ApiResponse> CreateOrder(CreateOrderModel model);

        Task<ApiResponse> CreateDriverTask(CreateDriverTaskModel model);

        IEnumerable<Order> GetAll(string searchColumn, string search);

        IEnumerable<Material> GetMaterials();

        IEnumerable<Order> GetAll(string sortColumn, string sortDirection, int pageNumber, int pageSize,
            string searchColumn, string search);

        IEnumerable<Order> GetAll(string sortColumn, string sortDirection, int pageNumber, int pageSize,
            string searchColumn, string search, DateTime startDate, DateTime endDate);

        Order GetOrderById(long orderId);
    }
}
