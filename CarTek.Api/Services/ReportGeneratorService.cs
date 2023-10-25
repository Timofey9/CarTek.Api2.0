using CarTek.Api.Model;
using CarTek.Api.Model.Dto;
using CarTek.Api.Model.Dto.Car;
using CarTek.Api.Model.Orders;
using CarTek.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.Formula;
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


        public MemoryStream GenerateTnsReport(IEnumerable<OrderModel> orders)
        {
            IWorkbook workbook;

            using (FileStream fileStream = new FileStream("/data/Templates/tnReportTemplate.xlsx", FileMode.Open, FileAccess.ReadWrite))
            {
                workbook = new XSSFWorkbook(fileStream);
            }

            // Получение листа
            ISheet sheet = workbook.GetSheetAt(0);
            int rowIndex = 4;

            var cellStyle = workbook.CreateCellStyle();
            cellStyle.Alignment = HorizontalAlignment.Center;
            cellStyle.WrapText = true;
            cellStyle.VerticalAlignment = VerticalAlignment.Center;
            cellStyle.BorderBottom = BorderStyle.Thin;
            cellStyle.BorderTop = BorderStyle.Thin;
            cellStyle.BorderLeft = BorderStyle.Thin;
            cellStyle.BorderRight = BorderStyle.Thin;

            foreach (var order in orders)
            {
                if(order.DriverTasks.Count > 0)
                {
                    foreach(var task in order.DriverTasks.Where(dt => dt.Status == DriverTaskStatus.Done))
                    {
                        var row = sheet.CreateRow(rowIndex);
                        
                        row.CreateCell(0).SetCellValue(order.StartDate.ToString("dd.MM.yyyy"));
                        row.GetCell(0).CellStyle = cellStyle;

                        var customer = order.Service == ServiceType.Supply ? order.Gp?.ClientName : order.Client?.ClientName;
                        row.CreateCell(1).SetCellValue(customer);
                        row.GetCell(1).CellStyle = cellStyle;

                        row.CreateCell(2).SetCellValue("КарТэк");
                        row.GetCell(2).CellStyle = cellStyle;

                        row.CreateCell(3).SetCellValue(order.Service == ServiceType.Supply ? "Поставка" : "Перевозка");
                        row.GetCell(3).CellStyle = cellStyle;

                        row.CreateCell(4).SetCellValue(task.TN?.Number);
                        row.GetCell(4).CellStyle = cellStyle;

                        row.CreateCell(5).SetCellValue(task.TN?.LocationA);
                        row.GetCell(5).CellStyle = cellStyle;

                        row.CreateCell(6).SetCellValue(task.TN?.LocationB);
                        row.GetCell(6).CellStyle = cellStyle;

                        row.CreateCell(7).SetCellValue(task.Car?.Plate);
                        row.GetCell(7).CellStyle = cellStyle;

                        row.CreateCell(8).SetCellValue(task.TN?.DriverInfo);
                        row.GetCell(8).CellStyle = cellStyle;

                        row.CreateCell(9).SetCellValue(task.TN?.Material);
                        row.GetCell(9).CellStyle = cellStyle;

                        row.CreateCell(10).SetCellValue(task.TN?.UnloadVolume);
                        row.GetCell(10).CellStyle = cellStyle;

                        row.CreateCell(11).SetCellValue(task.TN?.UnloadUnit);
                        row.GetCell(11).CellStyle = cellStyle;

                        row.CreateCell(12).SetCellValue(task.TN?.UnloadVolume2);
                        row.CreateCell(13).SetCellValue(task.TN?.UnloadUnit2);

                        row.CreateCell(14).SetCellValue(order.Price.ToString());

                        if(order.Service == ServiceType.Supply)
                        {
                            row.CreateCell(15).SetCellType(CellType.Formula);
                            row.GetCell(15).SetCellFormula($"O{rowIndex+1}*K{rowIndex+1}");
                            row.GetCell(15).CellStyle.WrapText = true;
                            row.CreateCell(16).SetCellFormula(order.MaterialPrice.ToString());
                            row.CreateCell(17).SetCellFormula($"Q{rowIndex+1}+O{rowIndex+1}");
                            row.CreateCell(18).SetCellFormula($"K{rowIndex+1}*R{rowIndex+1}");
                        }

                        rowIndex++;
                    }
                }
            }

            var stream = new MemoryStream();

            XSSFFormulaEvaluator.EvaluateAllFormulaCells(workbook);

            workbook.Write(stream, true);

            return stream;
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
            var cellStyle = workbook.CreateCellStyle();
            cellStyle.Alignment = HorizontalAlignment.Center;
            cellStyle.WrapText = true;
            cellStyle.VerticalAlignment = VerticalAlignment.Center;
            cellStyle.BorderBottom = BorderStyle.Thin;
            cellStyle.BorderTop = BorderStyle.Thin;
            cellStyle.BorderLeft = BorderStyle.Thin;
            cellStyle.BorderRight = BorderStyle.Thin;

            foreach (var order in orders)
            {
                var row = sheet.CreateRow(rowIndex);
                row.CreateCell(0).SetCellValue(order.StartDate.ToShortDateString());
                row.CreateCell(1).SetCellValue(order.Service == ServiceType.Supply ? "Поставка" : "Перевозка");
                if(order.Client != null)
                {
                    row.CreateCell(2).SetCellValue(order.Client.ClientName);
                    row.GetCell(2).CellStyle = cellStyle;
                }
                if (order.Gp != null)
                {
                    row.CreateCell(3).SetCellValue(order.Gp.ClientName);
                    row.GetCell(3).CellStyle = cellStyle;
                }
                row.CreateCell(4).SetCellValue(order.LocationA);
                row.GetCell(4).CellStyle = cellStyle;

                row.CreateCell(5).SetCellValue(order.LocationB);
                row.GetCell(5).CellStyle = cellStyle;

                row.CreateCell(6).SetCellValue(order.Material.Name);
                row.GetCell(6).CellStyle = cellStyle;

                row.CreateCell(7).SetCellValue($"{order.DriverTasks.Count}/{order.CarCount}");
                row.GetCell(7).CellStyle = cellStyle;

                rowIndex++;
            }

            for (var i = 2; i < 7; i++)
            {
                sheet.SetColumnWidth(i, 255 * 50);
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

            var cellStyle = workbook.CreateCellStyle();
            cellStyle.WrapText = true;
            cellStyle.VerticalAlignment = VerticalAlignment.Center;
            cellStyle.BorderBottom = BorderStyle.Thin;
            cellStyle.BorderTop = BorderStyle.Thin;
            cellStyle.BorderLeft = BorderStyle.Thin;
            cellStyle.BorderRight = BorderStyle.Thin;
            daterow.CreateCell(1).SetCellValue(date.ToString("dd.MM.yyyy"));

            foreach (var car in cars)
            {
                var row = sheet.CreateRow(rowIndex);

                foreach(var task in car.DriverTasks)
                {
                    row = sheet.CreateRow(rowIndex);
                    row.CreateCell(0).SetCellValue(car.Plate);
                    row.GetCell(0).CellStyle = cellStyle;

                    row.CreateCell(1).SetCellValue(task.Driver.FullName);
                    row.GetCell(1).CellStyle = cellStyle;

                    row.CreateCell(2).SetCellValue(ShiftToString(task.Shift));
                    row.GetCell(2).CellStyle = cellStyle;

                    row.CreateCell(3).SetCellValue(task.LocationA?.TextAddress);
                    row.GetCell(3).CellStyle = cellStyle;

                    row.CreateCell(4).SetCellValue(task.LocationB?.TextAddress);
                    row.GetCell(4).CellStyle = cellStyle;
                    rowIndex++;
                }
            }

            for(var i = 1; i < 5; i++)
            {
                sheet.SetColumnWidth(i, 255 * 50);
            }


            var stream = new MemoryStream();
            workbook.Write(stream, true);
            return stream;
        }


        public MemoryStream GenerateTasksReportShort(DateTime date, IEnumerable<CarDriverTaskModel> cars)
        {
            IWorkbook workbook;

            using (FileStream fileStream = new FileStream("/data/Templates/tasksShort.xlsx", FileMode.Open, FileAccess.ReadWrite))
            {
                workbook = new XSSFWorkbook(fileStream);
            }

            var cellStyle = workbook.CreateCellStyle();
            cellStyle.Alignment = HorizontalAlignment.Center;
            cellStyle.WrapText = true;
            cellStyle.VerticalAlignment = VerticalAlignment.Center;

            cellStyle.BorderBottom = BorderStyle.Thin;
            cellStyle.BorderTop = BorderStyle.Thin;
            cellStyle.BorderLeft = BorderStyle.Thin;
            cellStyle.BorderRight = BorderStyle.Thin;

            // Получение листа
            ISheet sheet = workbook.GetSheetAt(0);

            int numb = 1;

            int rowIndex = 4;

            var daterow = sheet.CreateRow(1);

            daterow.CreateCell(1).SetCellValue(date.ToString("dd.MM.yyyy"));

            foreach (var car in cars)
            {
                var row = sheet.CreateRow(rowIndex);

                row.CreateCell(0).SetCellValue(numb);
                row.GetCell(0).CellStyle = cellStyle;

                row.CreateCell(1).SetCellValue(car.Plate);
                row.GetCell(1).CellStyle = cellStyle;

                int driverRowIndex = rowIndex;

                for (var i = 2; i < 8; i++)
                {
                    row.CreateCell(i);
                    row.GetCell(i).CellStyle = cellStyle;
                }

                foreach (var task in car.DriverTasks)
                {
                    //var driverRow = sheet.CreateRow(driverRowIndex);
                    
                    var client = task.Order.Service == ServiceType.Supply ? task.Order.Gp.ClientName : task.Order.ClientName;

                    row.GetCell(2).SetCellValue(task.Driver.FullName);

                    row.GetCell(3).SetCellValue(client);

                    if (task.Shift == ShiftType.Night)
                    {
                        row.GetCell(4).SetCellValue("+");

                        //driverRow.CreateCell(3).SetCellValue("+");
                        //driverRow.GetCell(3).CellStyle = cellStyle;
                    }

                    if (task.Shift == ShiftType.Day)
                    {
                        row.GetCell(5).SetCellValue("+");

                        //driverRow.CreateCell(4).SetCellValue("+");
                        //driverRow.GetCell(4).CellStyle = cellStyle;
                    }

                    if (task.Shift == ShiftType.Fullday)
                    {
                        row.GetCell(6).SetCellValue("+");

                        //driverRow.CreateCell(5).SetCellValue("+");
                        //driverRow.GetCell(5).CellStyle = cellStyle;
                    }

                    if (task.Shift == ShiftType.Unlimited)
                    {
                        row.GetCell(7).SetCellValue("+");

                        //driverRow.CreateCell(6).SetCellValue("+");
                        //driverRow.GetCell(6).CellStyle = cellStyle;
                    }

                   // driverRowIndex++;
                }

                numb++;
                rowIndex++;
            }

            sheet.SetColumnWidth(1, 255 * 50);
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

            var cellStyle = workbook.CreateCellStyle();
            cellStyle.Alignment = HorizontalAlignment.Center;
            cellStyle.WrapText = true;
            cellStyle.VerticalAlignment = VerticalAlignment.Center;

            ISheet sheet = workbook.GetSheetAt(0);

            IRow row = sheet.GetRow(8);
            row.GetCell(6).SetCellValue(model.Date.ToString("dd.MM.yyyy"));

            row.GetCell(28).SetCellValue(model.Number);

            row = sheet.GetRow(14);
            row.GetCell(1).SetCellValue(model.GoInfo);
            row.GetCell(1).CellStyle = cellStyle;

            row = sheet.GetRow(19);
            row.GetCell(1).SetCellValue(model.GpInfo);
            row.GetCell(1).CellStyle = cellStyle;

            row = sheet.GetRow(21);
            row.GetCell(1).SetCellValue(model.LocationB);
            row.GetCell(1).CellStyle = cellStyle;

            row = sheet.GetRow(24);
            row.GetCell(1).SetCellValue(model.Material);
            row.GetCell(1).CellStyle = cellStyle;

            row.GetCell(57).SetCellValue(model.LoadVolume);

            row = sheet.GetRow(43);
            row.GetCell(1).SetCellValue("ООО \"КарТэк\"");
            row.GetCell(57).SetCellValue(model.DriverInfo);
            row.GetCell(57).CellStyle = cellStyle;

            row = sheet.GetRow(47);
            row.GetCell(1).SetCellValue(model.CarModel);
            row.GetCell(57).SetCellValue($"{model.CarPlate}/{model.TrailerPlate}");

            row = sheet.GetRow(55);
            row.GetCell(1).SetCellValue(model.GoInfo);
            row.GetCell(1).CellStyle = cellStyle;

            row = sheet.GetRow(59);
            row.GetCell(1).SetCellValue(model.LocationA);
            row.GetCell(1).CellStyle = cellStyle;

            row = sheet.GetRow(61);
            row.GetCell(1).SetCellValue(model.PickUpArrivalTime);
            row.GetCell(57).SetCellValue(model.PickUpDepartureTime);

            sheet = workbook.GetSheetAt(1);
            row = sheet.GetRow(2);
            row.GetCell(1).SetCellValue(model.LocationB);
            row.GetCell(1).CellStyle = cellStyle;

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
    }
}
