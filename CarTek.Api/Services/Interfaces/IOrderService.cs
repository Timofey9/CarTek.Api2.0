using CarTek.Api.Model;
using CarTek.Api.Model.Dto;
using CarTek.Api.Model.Orders;
using CarTek.Api.Model.Response;
using Microsoft.AspNetCore.JsonPatch;

namespace CarTek.Api.Services.Interfaces
{
    public interface IOrderService
    {
        Task<ApiResponse> CreateOrder(CreateOrderModel model);

        Task<ApiResponse> CreateDriverTask(CreateDriverTaskModel model);

        IEnumerable<Order> GetAllActive(DateTime startDate);

        IEnumerable<Material> GetMaterials();

        IEnumerable<Order> GetAll(string sortColumn, string sortDirection, int pageNumber, int pageSize,
            string searchColumn, string search);

        IEnumerable<Order> GetAll(string sortColumn, string sortDirection, int pageNumber, int pageSize,
            string searchColumn, string search, DateTime startDate, DateTime endDate);

        Order GetOrderById(long orderId);

        OrderExportModel GetOrderExportById(long orderId);

        ApiResponse DeleteOrder(long orderId);

        ApiResponse AddMaterial(string name);

        ApiResponse UpdateOrder(long orderId, JsonPatchDocument<Order> orderModel);

        IEnumerable<Order> GetAllBetweenDates(string? searchColumn, string? search, DateTime startDate, DateTime endDate);
       
        IEnumerable<OrderModel> GetOrderModelsBetweenDates(string? searchColumn, string? search, DateTime startDate, DateTime endDate);
    }
}
