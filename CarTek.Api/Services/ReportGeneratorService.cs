using CarTek.Api.Model;
using CarTek.Api.Model.Dto;
using CarTek.Api.Model.Dto.Car;
using CarTek.Api.Model.Orders;
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

        public MemoryStream GenerateOrdersReport(IEnumerable<OrderModel> orders)
        {
            IWorkbook workbook;

            using (FileStream fileStream = new FileStream("/data/Templates/reportTemplate.xlsx", FileMode.Open, FileAccess.ReadWrite))
            {
                workbook = new XSSFWorkbook(fileStream);
            }

            // Получение листа
            ISheet sheet = workbook.GetSheetAt(0);
            int rowIndex = 2;
            
            foreach(var order in orders)
            {
                var row = sheet.CreateRow(rowIndex);
                row.CreateCell(0).SetCellValue(order.StartDate.ToShortDateString());
                row.CreateCell(1).SetCellValue(order.Service == ServiceType.Supply ? "Поставка" : "Перевозка");
                if(order.Client != null)
                {
                    row.CreateCell(2).SetCellValue(order.Client.ClientName);
                }
                if (order.Gp != null)
                {
                    row.CreateCell(3).SetCellValue(order.Gp.ClientName);
                }
                row.CreateCell(4).SetCellValue(order.LocationA);
                row.CreateCell(5).SetCellValue(order.LocationB);
                row.CreateCell(6).SetCellValue(order.Material.Name);
                row.CreateCell(7).SetCellValue($"{order.DriverTasks.Count}/{order.CarCount}");

                rowIndex++;
            }

            var stream = new MemoryStream();

            workbook.Write(stream, true);

            return stream;
        }


        private string ShiftToString(ShiftType shift)
        {
            switch (shift)
            {
                case ShiftType.Day:
                    return "День (08:00 - 20:00)";
                case ShiftType.Night:
                    return "Ночь (20:00 - 08:00)";
                case ShiftType.Fullday:
                    return "Сутки";
                case ShiftType.Unlimited:
                    return "Сутки (не ограничено)";
                default:
                    return " ";
            }
        }

        public MemoryStream GenerateTasksReport(DateTime date, IEnumerable<CarDriverTaskModel> cars)
        {
            IWorkbook workbook;

            using (FileStream fileStream = new FileStream("/data/Templates/tasksReport.xlsx", FileMode.Open, FileAccess.ReadWrite))
            {
                workbook = new XSSFWorkbook(fileStream);
            }

            // Получение листа
            ISheet sheet = workbook.GetSheetAt(0);

            int rowIndex = 4;

            var daterow = sheet.CreateRow(1);

            daterow.CreateCell(1).SetCellValue(date.ToString("dd.MM.yyyy"));

            foreach (var car in cars)
            {
                var row = sheet.CreateRow(rowIndex);

                foreach(var task in car.DriverTasks)
                {
                    row.CreateCell(0).SetCellValue(car.Plate);
                    row.CreateCell(1).SetCellValue(task.Driver.FullName);
                    row.CreateCell(2).SetCellValue(ShiftToString(task.Shift));
                    row.CreateCell(3).SetCellValue(task.LocationA?.TextAddress);
                    row.CreateCell(4).SetCellValue(task.LocationB?.TextAddress);

                    rowIndex++;
                }
            }

            var stream = new MemoryStream();
            workbook.Write(stream, true);
            return stream;
        }

        public MemoryStream GenerateTn(TNModel model)
        {
            IWorkbook workbook;

            using (FileStream fileStream = new FileStream("/data/Templates/TN.xlsx", FileMode.Open, FileAccess.ReadWrite))
            {
                workbook = new XSSFWorkbook(fileStream);
            }

            ISheet sheet = workbook.GetSheetAt(0);

            IRow row = sheet.GetRow(8);
            row.GetCell(6).SetCellValue(model.Date.ToString("dd.MM.yyyy"));
            row.GetCell(28).SetCellValue(model.Number);


            row = sheet.GetRow(14);
            row.GetCell(1).SetCellValue(model.GoInfo);

            row = sheet.GetRow(19);
            row.GetCell(1).SetCellValue(model.GpInfo);

            row = sheet.GetRow(21);
            row.GetCell(1).SetCellValue(model.LocationB);

            row = sheet.GetRow(24);
            row.GetCell(1).SetCellValue(model.Material);
            row.GetCell(57).SetCellValue(model.LoadVolume);

            row = sheet.GetRow(43);
            row.GetCell(1).SetCellValue("ООО \"КарТэк\"");
            row.GetCell(57).SetCellValue(model.DriverInfo);

            row = sheet.GetRow(47);
            row.GetCell(1).SetCellValue(model.CarModel);
            row.GetCell(57).SetCellValue($"{model.CarPlate}/{model.TrailerPlate}");


            row = sheet.GetRow(55);
            row.GetCell(1).SetCellValue(model.GoInfo);

            row = sheet.GetRow(59);
            row.GetCell(1).SetCellValue(model.LocationA);

            row = sheet.GetRow(61);
            row.GetCell(1).SetCellValue(model.PickUpArrivalTime);
            row.GetCell(57).SetCellValue(model.PickUpDepartureTime);

            sheet = workbook.GetSheetAt(1);
            row = sheet.GetRow(2);
            row.GetCell(1).SetCellValue(model.LocationB);

            row = sheet.GetRow(4);
            row.GetCell(1).SetCellValue(model.DropOffArrivalTime);
            row.GetCell(57).SetCellValue(model.DropOffDepartureTime);

            //row = sheet.GetRow(52);
            //row.GetCell(1).SetCellValue(model.PickUpArrivalTime);
            //row.GetCell(15).SetCellValue(model.PickUpDepartureTime);
            //row.GetCell(29).SetCellValue(model.DropOffArrivalTime);
            //row.GetCell(45).SetCellValue(model.PickUpDepartureTime);

            //row = sheet.GetRow(56);
            //row.GetCell(1).SetCellValue(model.LoadVolume);
            //row.GetCell(29).SetCellValue(model.UnloadVolume);

            //row = sheet.GetRow(60);
            //row.GetCell(1).SetCellValue(model.DriverInfo);
            //row.GetCell(29).SetCellValue(model.DriverInfo);

            //row = sheet.GetRow(76);
            //row.GetCell(0).SetCellValue(model.Date.ToShortDateString());
            //row.GetCell(15).SetCellValue("");

            //row = sheet.GetRow(85);
            //row.GetCell(1).SetCellValue(model.DriverInfo);



            var stream = new MemoryStream();
            workbook.Write(stream, true);
            return stream;
        }

        public MemoryStream TestGenerateReport(string input)
        {
            IWorkbook workbook;
         
            using (FileStream fileStream = new FileStream("/data/Templates/reportTemplate.xlsx", FileMode.Open, FileAccess.ReadWrite))
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
