using CarTek.Api.Model;
using CarTek.Api.Model.Dto;
using CarTek.Api.Model.Dto.Car;
using Microsoft.AspNetCore.Mvc;

namespace CarTek.Api.Services.Interfaces
{
    public interface IReportGeneratorService
    {
        public MemoryStream GenerateOrdersReport(IEnumerable<OrderModel> orders, DateTime startDate, DateTime endDate);

        public MemoryStream GenerateTnsReport(IEnumerable<TNModel> tns, DateTime startDate, DateTime endDate);

        public MemoryStream GenerateSalariesReport(IEnumerable<TNModel> tns, DateTime startDate, DateTime endDate);

        public MemoryStream GenerateTasksReport(DateTime date, IEnumerable<CarDriverTaskModel> tasks);

        public MemoryStream GenerateTasksReportFull(DateTime startDate, DateTime endDate, IEnumerable<DriverTaskReportModel> tasks);

        public MemoryStream GenerateTasksReportShort(DateTime date, IEnumerable<CarDriverTaskModel> tasks);

        public MemoryStream GenerateTn(TNModel model);
    }
}
