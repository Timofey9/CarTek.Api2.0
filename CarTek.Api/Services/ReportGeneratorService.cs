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
