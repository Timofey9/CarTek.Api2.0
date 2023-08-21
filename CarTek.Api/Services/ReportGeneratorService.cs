using CarTek.Api.Model;
using CarTek.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.IO;

namespace CarTek.Api.Services
{
    public class ReportGeneratorService : IReportGeneratorService
    {
        private readonly ILogger<ReportGeneratorService> _logger;

        public ReportGeneratorService(ILogger<ReportGeneratorService> logger)
        {
            _logger = logger;
        }

        public MemoryStream GenerateOrdersReport(IEnumerable<Order> orders)
        {
            IWorkbook workbook;

            using (FileStream fileStream = new FileStream("\\Templates\\reportTemplate.xlsx", FileMode.Open, FileAccess.ReadWrite))
            {
                workbook = new XSSFWorkbook(fileStream);
            }

            // Получение листа
            ISheet sheet = workbook.GetSheetAt(0);
            int rowIndex = 3;
            
            foreach(var order in orders)
            {
                var row = sheet.CreateRow(rowIndex);
                row.CreateCell(0).SetCellValue(order.Id.ToString());
                row.CreateCell(1).SetCellValue(order.ClientName);
                row.CreateCell(2).SetCellValue(order.StartDate.ToShortDateString());
                row.CreateCell(3).SetCellValue(order.Material.Name);
                row.CreateCell(4).SetCellValue($"{order.DriverTasks.Count}/{order.CarCount}");

                rowIndex++;
            }

            var stream = new MemoryStream();

            workbook.Write(stream, true);

            return stream;
        }

        public MemoryStream GenerateTn(TNModel model)
        {
            IWorkbook workbook;

            using (FileStream fileStream = new FileStream("\\Templates\\TN.xlsx", FileMode.Open, FileAccess.ReadWrite))
            {
                workbook = new XSSFWorkbook(fileStream);
            }

            ISheet sheet = workbook.GetSheetAt(0);

            IRow row = sheet.GetRow(12);

            row.GetCell(1).SetCellValue(model.Sender);
            row.GetCell(29).SetCellValue(model.ClientInfo);

            row = sheet.GetRow(20);
            row.GetCell(1).SetCellValue(model.Material);

            row = sheet.GetRow(22);
            row.GetCell(1).SetCellValue(model.MaterialAmount);

            row = sheet.GetRow(48);
            row.GetCell(1).SetCellValue(model.LocationA);
            row.GetCell(29).SetCellValue(model.LocationB);

            row = sheet.GetRow(60);
            row.GetCell(1).SetCellValue(model.DriverInfo);
            row.GetCell(29).SetCellValue(model.DriverInfo);

            row = sheet.GetRow(90);
            row.GetCell(1).SetCellValue(model.CarModel);

            row = sheet.GetRow(92);
            row.GetCell(29).SetCellValue($"{model.CarPlate}/{model.TrailerPlate}");

            row = sheet.GetRow(123);
            row.GetCell(1).SetCellValue("ООО \"КарТэк\"");

            var stream = new MemoryStream();
            workbook.Write(stream, true);
            return stream;
        }

        public MemoryStream TestGenerateReport(string input)
        {
            IWorkbook workbook;
         
            using (FileStream fileStream = new FileStream("\\Templates\\reportTemplate.xlsx", FileMode.Open, FileAccess.ReadWrite))
            {
                workbook = new XSSFWorkbook(fileStream);
            }

            // Получение листа
            ISheet sheet = workbook.GetSheetAt(0);

            // Обновление данных ячейки
            IRow row = sheet.GetRow(0);
            row.GetCell(5).SetCellValue(input);

            var stream = new MemoryStream();

            workbook.Write(stream, true);

            return stream;

            //using (MemoryStream ms = new MemoryStream())
            //{
            //    workbook.Write(ms, false);

            //    return ms.ToArray();
            //}
        }
    }
}
