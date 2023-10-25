using CarTek.Api.Model;
using CarTek.Api.Model.Dto;
using CarTek.Api.Model.Dto.Car;
using Microsoft.AspNetCore.Mvc;

namespace CarTek.Api.Services.Interfaces
{
    public interface IReportGeneratorService
    {
        public MemoryStream GenerateOrdersReport(IEnumerable<OrderModel> orders);

        public MemoryStream GenerateTnsReport(IEnumerable<OrderModel> orders);

        public MemoryStream GenerateTasksReport(DateTime date, IEnumerable<CarDriverTaskModel> tasks);

        public MemoryStream GenerateTasksReportShort(DateTime date, IEnumerable<CarDriverTaskModel> tasks);

        public MemoryStream GenerateTn(TNModel model);
    }
}
