using CarTek.Api.Model;
using Microsoft.AspNetCore.Mvc;

namespace CarTek.Api.Services.Interfaces
{
    public interface IReportGeneratorService
    {
        public MemoryStream TestGenerateReport(string input);

        public MemoryStream GenerateOrdersReport(IEnumerable<Order> orders);
    }
}
