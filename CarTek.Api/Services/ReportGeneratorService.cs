using CarTek.Api.Model;
using CarTek.Api.Model.Dto;
using CarTek.Api.Model.Dto.Car;
using CarTek.Api.Model.Orders;
using CarTek.Api.Services.Interfaces;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Globalization;

namespace CarTek.Api.Services
{
    public class ReportGeneratorService : IReportGeneratorService
    {
        private readonly ILogger<ReportGeneratorService> _logger;

        public ReportGeneratorService(ILogger<ReportGeneratorService> logger)
        {
            _logger = logger;
        }

        private string StatusToString(DriverTaskStatus status)
        {
            switch (status)
            {
                case DriverTaskStatus.Assigned:
                    return "Назначена";
                case DriverTaskStatus.Confirmed:
                    return "Принята";
                case DriverTaskStatus.OnRoute:
                    return "На линии";
                case DriverTaskStatus.Loading:
                    return "Прибыл на склад загрузки";
                case DriverTaskStatus.DocumentSigning1:
                    return "Выписка ТН (первая часть)";
                case DriverTaskStatus.OutLoad:
                    return "Выехал со складка погрузки";
                case DriverTaskStatus.ArrivedToUnload:
                    return "Прибыл на объект выгрузки";
                case DriverTaskStatus.Unloading:
                    return "Выгрузка";
                case DriverTaskStatus.DocumentSigning2:
                    return "Выписка ТН (вторая часть)";
                case DriverTaskStatus.Done:
                    return "Завершена";
                default:
                    return status.ToString();
            }
        }

        public MemoryStream GenerateTnsReport(IEnumerable<TNModel> tns, DateTime startDate, DateTime endDate)
        {
            IWorkbook workbook;

            using (FileStream fileStream = new FileStream("/data/Templates/tnReportTemplate.xlsx", FileMode.Open, FileAccess.ReadWrite))
            {
                workbook = new XSSFWorkbook(fileStream);
            }

            // Получение листа
            ISheet sheet = workbook.GetSheetAt(0);

            var dateRow = sheet.GetRow(1);
            dateRow.GetCell(2).SetCellValue($"{startDate.ToString("dd.MM.yyyy")}-{endDate.ToString("dd.MM.yyyy")}");

            int rowIndex = 4;
            var cellStyle = workbook.CreateCellStyle();
            cellStyle.Alignment = HorizontalAlignment.Center;
            cellStyle.WrapText = true;
            cellStyle.VerticalAlignment = VerticalAlignment.Center;
            cellStyle.BorderBottom = BorderStyle.Thin;
            cellStyle.BorderTop = BorderStyle.Thin;
            cellStyle.BorderLeft = BorderStyle.Thin;
            cellStyle.BorderRight = BorderStyle.Thin;

            foreach (var tn in tns)
            {
                var row = sheet.CreateRow(rowIndex);

                row.CreateCell(0).SetCellValue(tn.PickUpDepartureTime);
                row.GetCell(0).CellStyle = cellStyle;
                row.CreateCell(1).SetCellValue(tn.DropOffDepartureTime);
                row.GetCell(1).CellStyle = cellStyle;

                row.CreateCell(2).SetCellValue(tn.Client);
                row.GetCell(2).CellStyle = cellStyle;

                //TODO:
                row.CreateCell(3).SetCellValue("КарТэк");
                row.GetCell(3).CellStyle = cellStyle;

                //TODO:
                row.CreateCell(4).SetCellValue(tn.Order.Service == ServiceType.Supply ? "Поставка" : "Перевозка");
                row.GetCell(4).CellStyle = cellStyle;

                row.CreateCell(5).SetCellValue(tn.Number);
                row.GetCell(5).CellStyle = cellStyle;

                row.CreateCell(6).SetCellValue(tn.LocationA);
                row.GetCell(6).CellStyle = cellStyle;

                row.CreateCell(7).SetCellValue(tn.LocationB);
                row.GetCell(7).CellStyle = cellStyle;

                row.CreateCell(8).SetCellValue(tn.CarPlate);
                row.GetCell(8).CellStyle = cellStyle;

                row.CreateCell(9).SetCellValue(tn.DriverInfo);
                row.GetCell(9).CellStyle = cellStyle;

                row.CreateCell(10).SetCellValue(StatusToString(tn.TaskStatus));
                row.GetCell(10).CellStyle = cellStyle;

                row.CreateCell(11).SetCellValue(tn.Material);
                row.GetCell(11).CellStyle = cellStyle;

                row.CreateCell(12).SetCellValue(tn.UnloadVolume);
                row.GetCell(12).CellStyle = cellStyle;

                row.CreateCell(13).SetCellValue(tn.UnloadUnit);
                row.GetCell(13).CellStyle = cellStyle;

                row.CreateCell(14).SetCellValue(tn.Order.Price.ToString());

                row.CreateCell(15).SetCellType(CellType.Formula);
                row.GetCell(15).SetCellFormula($"O{rowIndex + 1}*M{rowIndex + 1}");
                row.GetCell(15).CellStyle.WrapText = true;
                row.CreateCell(16).SetCellValue(tn.Order.MaterialPrice.ToString());
                row.CreateCell(17).SetCellFormula($"Q{rowIndex + 1}+O{rowIndex + 1}");
                row.CreateCell(18).SetCellFormula($"M{rowIndex + 1}*R{rowIndex + 1}");

                row.CreateCell(21).SetCellValue(tn.IsVerified ? "Да" : "Нет");
                row.CreateCell(22).SetCellValue(tn.IsOriginalReceived ? "Да" : "Нет");

                rowIndex++;
            }

            var stream = new MemoryStream();

            for(var i=0; i < 21; i++)
            {
                sheet.SetColumnWidth(i, 255 * 40);
            }

            var lst = tns.ToList();

            for (var i = 4; i < rowIndex; i++)
            {
                if (lst[i - 4].TaskStatus == DriverTaskStatus.Done)
                {
                    var row = sheet.GetRow(i);
                    var cell = row.GetCell(10);
                    var cellStyle2 = workbook.CreateCellStyle();


                    cellStyle2.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightGreen.Index;
                    cellStyle2.FillPattern = FillPattern.SolidForeground;

                    cellStyle2.BorderBottom = BorderStyle.Thin;
                    cellStyle2.BorderTop = BorderStyle.Thin;
                    cellStyle2.BorderLeft = BorderStyle.Thin;
                    cellStyle2.BorderRight = BorderStyle.Thin;

                    cell.CellStyle = cellStyle2;
                }
                else 
                {
                    var row = sheet.GetRow(i);
                    var cell = row.GetCell(10);
                    var cellStyle2 = workbook.CreateCellStyle();

                    cellStyle2.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightYellow.Index;
                    cellStyle2.FillPattern = FillPattern.SolidForeground;

                    cellStyle2.BorderBottom = BorderStyle.Thin;
                    cellStyle2.BorderTop = BorderStyle.Thin;
                    cellStyle2.BorderLeft = BorderStyle.Thin;
                    cellStyle2.BorderRight = BorderStyle.Thin;

                    cell.CellStyle = cellStyle2;
                }
            }

            XSSFFormulaEvaluator.EvaluateAllFormulaCells(workbook);

            workbook.Write(stream, true);

            return stream;
        }

        public MemoryStream GenerateOrdersReport(IEnumerable<OrderModel> orders, DateTime startDate, DateTime endDate)
        {
            IWorkbook workbook;

            using (FileStream fileStream = new FileStream("/data/Templates/reportTemplate.xlsx", FileMode.Open, FileAccess.ReadWrite))
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

            int number = 1;

            var dateRow = sheet.GetRow(1);
            dateRow.GetCell(4).SetCellValue($"{startDate.ToString("dd.MM.yyyy")}-{endDate.ToString("dd.MM.yyyy")}");

            foreach (var order in orders)
            {
                var row = sheet.CreateRow(rowIndex);

                row.CreateCell(0).SetCellValue(number.ToString());
                row.GetCell(0).CellStyle = cellStyle;

                row.CreateCell(1).SetCellValue(order.StartDate.ToString("dd.MM.yyyy"));
                row.GetCell(1).CellStyle = cellStyle;

                row.CreateCell(2).SetCellValue(ShiftToString(order.Shift));
                row.GetCell(2).CellStyle = cellStyle;

                row.CreateCell(3).SetCellValue(order.Service == ServiceType.Supply ? "Поставка" : "Перевозка");
                row.GetCell(3).CellStyle = cellStyle;

                if (order.Client != null)
                {
                    row.CreateCell(4).SetCellValue(order.Client.ClientName);
                    row.GetCell(4).CellStyle = cellStyle;
                }
                if (order.Gp != null)
                {
                    row.CreateCell(5).SetCellValue(order.Gp.ClientName);
                    row.GetCell(5).CellStyle = cellStyle;
                }
                        
                row.CreateCell(6).SetCellValue(order.Service == ServiceType.Supply ? order.Gp?.ClientName : order.Client?.ClientName);                
                row.GetCell(6).CellStyle = cellStyle;                

                row.CreateCell(7).SetCellValue(order.LocationA);
                row.GetCell(7).CellStyle = cellStyle;

                row.CreateCell(8).SetCellValue(order.LocationB);
                row.GetCell(8).CellStyle = cellStyle;

                row.CreateCell(9).SetCellValue(order.Material?.Name);
                row.GetCell(9).CellStyle = cellStyle;

                row.CreateCell(10).SetCellValue($"{order.DriverTasks.Count}/{order.CarCount}");

                rowIndex++;
                number++;
            }

            for (var i = 2; i < 7; i++)
            {
                sheet.SetColumnWidth(i, 255 * 50);
            }

            var lst = orders.ToList();

            for(var i = 4; i < rowIndex; i++)
            {
                if (lst[i-4].DriverTasks.Count >= lst[i-4].CarCount)
                {
                    var row = sheet.GetRow(i);
                    var cell = row.GetCell(10);
                    var cellStyle2 = workbook.CreateCellStyle();


                    cellStyle2.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightGreen.Index;
                    cellStyle2.FillPattern = FillPattern.SolidForeground;

                    cellStyle2.BorderBottom = BorderStyle.Thin;
                    cellStyle2.BorderTop = BorderStyle.Thin;
                    cellStyle2.BorderLeft = BorderStyle.Thin;
                    cellStyle2.BorderRight = BorderStyle.Thin;

                    cell.CellStyle = cellStyle2;
                }
                else
                {
                    var row = sheet.GetRow(i);
                    var cell = row.GetCell(10);
                    var cellStyle2 = workbook.CreateCellStyle();

                    cellStyle2.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Red.Index;
                    cellStyle2.FillPattern = FillPattern.SolidForeground;

                    cellStyle2.BorderBottom = BorderStyle.Thin;
                    cellStyle2.BorderTop = BorderStyle.Thin;
                    cellStyle2.BorderLeft = BorderStyle.Thin;
                    cellStyle2.BorderRight = BorderStyle.Thin;

                    cell.CellStyle = cellStyle2;
                }
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

        public MemoryStream GenerateTasksReportFull(DateTime startDate, DateTime endDate, IEnumerable<DriverTaskReportModel> tasks)
        {
            IWorkbook workbook;

            using (FileStream fileStream = new FileStream("/data/Templates/fullTasksReport.xlsx", FileMode.Open, FileAccess.ReadWrite))
            {
                workbook = new XSSFWorkbook(fileStream);
            }

            // Получение листа
            ISheet sheet = workbook.GetSheetAt(0);

            int rowIndex = 4;
            int num = 1;

            var cellStyle = workbook.CreateCellStyle();
            cellStyle.WrapText = true;
            cellStyle.VerticalAlignment = VerticalAlignment.Center;
            cellStyle.BorderBottom = BorderStyle.Thin;
            cellStyle.BorderTop = BorderStyle.Thin;
            cellStyle.BorderLeft = BorderStyle.Thin;
            cellStyle.BorderRight = BorderStyle.Thin;

            var daterow = sheet.GetRow(1);
            daterow.GetCell(3).SetCellValue($"{startDate.ToString("dd.MM.yyyy")}-{endDate.ToString("dd.MM.yyyy")}");

            foreach (var task in tasks)
            {
                var row = sheet.CreateRow(rowIndex);
                    
                row = sheet.CreateRow(rowIndex);
                
                row.CreateCell(0).SetCellValue(num.ToString());                
                row.GetCell(0).CellStyle = cellStyle;

                row.CreateCell(1).SetCellValue(task.Plate);
                row.GetCell(1).CellStyle = cellStyle;

                row.CreateCell(2).SetCellValue(task.Driver);                
                row.GetCell(2).CellStyle = cellStyle;
                
                row.CreateCell(3).SetCellValue(task.Service);                
                row.GetCell(3).CellStyle = cellStyle;

                row.CreateCell(4).SetCellValue(task.Go);
                row.GetCell(4).CellStyle = cellStyle;

                row.CreateCell(5).SetCellValue(task.Gp);
                row.GetCell(5).CellStyle = cellStyle;

                row.CreateCell(6).SetCellValue(task.Client);
                row.GetCell(6).CellStyle = cellStyle;

                row.CreateCell(7).SetCellValue(task.LocationA);
                row.GetCell(7).CellStyle = cellStyle;

                row.CreateCell(8).SetCellValue(task.LocationB);
                row.GetCell(8).CellStyle = cellStyle;

                row.CreateCell(9).SetCellValue(task.Material);
                row.GetCell(9).CellStyle = cellStyle;

                row.CreateCell(10).SetCellValue(ShiftToString(task.Shift));
                row.GetCell(10).CellStyle = cellStyle;

                row.CreateCell(11).SetCellValue(StatusToString(task.Status));
                row.GetCell(11).CellStyle = cellStyle;

                row.CreateCell(12).SetCellValue(task.OrderComment);
                row.GetCell(12).CellStyle = cellStyle;

                row.CreateCell(13).SetCellValue(task.TaskComment);
                row.GetCell(13).CellStyle = cellStyle;

                rowIndex++;
                num++;       
            }



            sheet.SetColumnWidth(1, 255 * 30);
            sheet.SetColumnWidth(2, 255 * 50);
            sheet.SetColumnWidth(3, 255 * 20);

            for (var i = 4; i < 8; i++)
            {
                sheet.SetColumnWidth(i, 255 * 50);
            }
            sheet.SetColumnWidth(9, 255 * 30);
            sheet.SetColumnWidth(10, 255 * 20);
            sheet.SetColumnWidth(11, 255 * 20);
            sheet.SetColumnWidth(12, 255 * 50);
            sheet.SetColumnWidth(13, 255 * 50);


            var lst = tasks.ToList();

            for (var i = 4; i < rowIndex; i++)
            {
                if (lst[i-4].Status == DriverTaskStatus.Done)
                {
                    var row = sheet.GetRow(i);
                    var cell = row.GetCell(11);
                    var cellStyle2 = workbook.CreateCellStyle();


                    cellStyle2.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightGreen.Index;
                    cellStyle2.FillPattern = FillPattern.SolidForeground;

                    cellStyle2.BorderBottom = BorderStyle.Thin;
                    cellStyle2.BorderTop = BorderStyle.Thin;
                    cellStyle2.BorderLeft = BorderStyle.Thin;
                    cellStyle2.BorderRight = BorderStyle.Thin;

                    cell.CellStyle = cellStyle2;
                }
                else if (lst[i-4].Status == DriverTaskStatus.Assigned)
                {
                    var row = sheet.GetRow(i);
                    var cell = row.GetCell(11);
                    var cellStyle2 = workbook.CreateCellStyle();

                    cellStyle2.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Red.Index;
                    cellStyle2.FillPattern = FillPattern.SolidForeground;

                    cellStyle2.BorderBottom = BorderStyle.Thin;
                    cellStyle2.BorderTop = BorderStyle.Thin;
                    cellStyle2.BorderLeft = BorderStyle.Thin;
                    cellStyle2.BorderRight = BorderStyle.Thin;

                    cell.CellStyle = cellStyle2;
                }
                else
                {
                    var row = sheet.GetRow(i);
                    var cell = row.GetCell(11);
                    var cellStyle2 = workbook.CreateCellStyle();

                    cellStyle2.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightYellow.Index;
                    cellStyle2.FillPattern = FillPattern.SolidForeground;

                    cellStyle2.BorderBottom = BorderStyle.Thin;
                    cellStyle2.BorderTop = BorderStyle.Thin;
                    cellStyle2.BorderLeft = BorderStyle.Thin;
                    cellStyle2.BorderRight = BorderStyle.Thin;

                    cell.CellStyle = cellStyle2;
                }
            }


            var stream = new MemoryStream();
            workbook.Write(stream, true);
            return stream;
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

                for (var i = 2; i < 14; i++)
                {
                    row.CreateCell(i);
                    row.GetCell(i).CellStyle = cellStyle;
                }

                foreach (var task in car.DriverTasks)
                {
                    //var driverRow = sheet.CreateRow(driverRowIndex);
                    
                    var client = task.Order.Service == ServiceType.Supply ? task.Order.Gp.ClientName : task.Order.ClientName;

                    row.GetCell(2).SetCellValue(task.Driver.FullName);

                    row.GetCell(3).SetCellValue(task.Order.Service == ServiceType.Supply ? "Поставка" : "Перевозка");

                    row.GetCell(4).SetCellValue(task.Order.ClientName);
                    row.GetCell(5).SetCellValue(task.Order.Gp.ClientName);

                    row.GetCell(6).SetCellValue(client);

                    row.GetCell(7).SetCellValue(task.Order.Material.Name);

                    row.GetCell(8).SetCellValue(task.LocationA.TextAddress);
                    row.GetCell(9).SetCellValue(task.LocationB.TextAddress);

                    if (task.Shift == ShiftType.Night)
                    {
                        row.GetCell(10).SetCellValue("+");

                        //driverRow.CreateCell(3).SetCellValue("+");
                        //driverRow.GetCell(3).CellStyle = cellStyle;
                    }

                    if (task.Shift == ShiftType.Day)
                    {
                        row.GetCell(11).SetCellValue("+");

                        //driverRow.CreateCell(4).SetCellValue("+");
                        //driverRow.GetCell(4).CellStyle = cellStyle;
                    }

                    if (task.Shift == ShiftType.Fullday)
                    {
                        row.GetCell(12).SetCellValue("+");

                        //driverRow.CreateCell(5).SetCellValue("+");
                        //driverRow.GetCell(5).CellStyle = cellStyle;
                    }

                    if (task.Shift == ShiftType.Unlimited)
                    {
                        row.GetCell(13).SetCellValue("+");

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

        private string UnitToString(Unit? unit)
        {
            switch (unit)
            {
                case Unit.t:
                    return "тн";
                case Unit.m3:
                    return "m3";
                default:
                    return "m3";
            }
        }


        public MemoryStream GenerateTn(TNModel model)
        {
            IWorkbook workbook;

            using (FileStream fileStream = new FileStream("/data/Templates/TN.xlsx", FileMode.Open, FileAccess.ReadWrite))
            {
                workbook = new XSSFWorkbook(fileStream);
            }

            IFont font = workbook.CreateFont();
            font.FontName = "Times New Roman";
            font.FontHeightInPoints = 7;
            font.Color = HSSFColor.Black.Index;

            var cellStyle = workbook.CreateCellStyle();
            cellStyle.Alignment = HorizontalAlignment.Center;
            cellStyle.WrapText = true;
            cellStyle.VerticalAlignment = VerticalAlignment.Center;
            cellStyle.SetFont(font);

            ISheet sheet = workbook.GetSheetAt(0);

            IRow row = sheet.GetRow(8);
            row.GetCell(6).SetCellValue(model.Date.ToString("dd.MM.yyyy"));

            row.GetCell(28).SetCellValue(model.Number);
            row.Height = 200;

            row = sheet.GetRow(14);
            row.Height = 200;
            row.GetCell(1).SetCellValue(model.GoInfo);
            row.GetCell(1).CellStyle = cellStyle;

            row = sheet.GetRow(19);
            row.Height = 200;
            row.GetCell(1).SetCellValue(model.GpInfo);
            row.GetCell(1).CellStyle = cellStyle;

            row = sheet.GetRow(21);
            row.Height = 200;
            row.GetCell(1).SetCellValue(model.LocationB);
            row.GetCell(1).CellStyle = cellStyle;

            row = sheet.GetRow(24);
            row.Height = 200;
            row.GetCell(1).SetCellValue(model.Material);
            row.GetCell(1).CellStyle = cellStyle;

            row = sheet.GetRow(43);
            row.Height = 200;
            row.GetCell(1).SetCellValue("ООО \"КарТэк\"");
            row.GetCell(1).CellStyle = cellStyle;

            row.GetCell(57).SetCellValue(model.DriverInfo);
            row.GetCell(57).CellStyle = cellStyle;

            row = sheet.GetRow(47);
            row.Height = 200;
            row.GetCell(1).SetCellValue(model.CarModel);
            row.GetCell(1).CellStyle = cellStyle;

            row.GetCell(57).SetCellValue($"{model.CarPlate}/{model.TrailerPlate}");
            row.GetCell(57).CellStyle = cellStyle;          

            sheet = workbook.GetSheetAt(1);

            row = sheet.GetRow(2);
            row.Height = 200;
            row.GetCell(1).SetCellValue(model.GoInfo);
            row.GetCell(1).CellStyle = cellStyle;

            row = sheet.GetRow(6);
            row.Height = 400;
            row.GetCell(1).SetCellValue(model.LocationA);
            row.GetCell(1).CellStyle = cellStyle;

            row = sheet.GetRow(8);
            row.Height = 200;
            row.GetCell(1).SetCellValue(model.PickUpArrivalTime);
            row.GetCell(1).CellStyle = cellStyle;
            row.GetCell(57).SetCellValue(model.PickUpDepartureTime);
            row.GetCell(57).CellStyle = cellStyle;

            string loadVolume = $"{model.LoadVolume} {model.Unit}";
            if (model.LoadVolume2 != null && model.LoadVolume2 != "0")
            {
                loadVolume += $", {model.LoadVolume2} {model.Unit2}";
            }
            row = sheet.GetRow(12);
            row.Height = 200;
            row.GetCell(1).SetCellValue(loadVolume);
            row.GetCell(1).CellStyle = cellStyle;

            row = sheet.GetRow(25);
            row.Height = 400;
            row.GetCell(1).SetCellValue(model.LocationB);
            row.GetCell(1).CellStyle = cellStyle;

            row = sheet.GetRow(27);
            row.Height = 200;
            row.GetCell(1).SetCellValue(model.DropOffArrivalTime);
            row.GetCell(1).CellStyle = cellStyle;

            row.GetCell(57).SetCellValue(model.DropOffDepartureTime);
            row.GetCell(57).CellStyle = cellStyle;

            string unloadVolume = $"{model.UnloadVolume} {model.UnloadUnit}";
            if (model.UnloadVolume2 != null && model.UnloadVolume2 != "0")
            {
                unloadVolume += $", {model.UnloadVolume2} {model.UnloadUnit2}";
            }

            row = sheet.GetRow(29);
            row.Height = 200;
            row.GetCell(57).SetCellValue(unloadVolume);
            row.GetCell(57).CellStyle = cellStyle;

            var stream = new MemoryStream();
            workbook.Write(stream, true);
            return stream;
        }

        public MemoryStream GenerateSalariesReport(IEnumerable<TNModel> tns, DateTime startDate, DateTime endDate)
        {
            IWorkbook workbook;

            using (FileStream fileStream = new FileStream("/data/Templates/reportAccountant.xlsx", FileMode.Open, FileAccess.ReadWrite))
            {
                workbook = new XSSFWorkbook(fileStream);
            }

            // Получение листа 
            ISheet sheet = workbook.GetSheetAt(0);

            var dateRow = sheet.GetRow(1);
            dateRow.GetCell(2).SetCellValue($"{startDate.ToString("dd.MM.yyyy")}-{endDate.ToString("dd.MM.yyyy")}");
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ",";
            int rowIndex = 4;
            var cellStyle = workbook.CreateCellStyle();
            cellStyle.Alignment = HorizontalAlignment.Center;
            cellStyle.WrapText = true;
            cellStyle.VerticalAlignment = VerticalAlignment.Center;
            cellStyle.BorderBottom = BorderStyle.Thin;
            cellStyle.BorderTop = BorderStyle.Thin;
            cellStyle.BorderLeft = BorderStyle.Thin;
            cellStyle.BorderRight = BorderStyle.Thin;            

            foreach (var tn in tns)
            {
                var row = sheet.CreateRow(rowIndex);

                row.CreateCell(0).SetCellValue(tn.PickUpDepartureTime);
                row.GetCell(0).CellStyle = cellStyle;
                row.CreateCell(1).SetCellValue(tn.DropOffDepartureTime);
                row.GetCell(1).CellStyle = cellStyle;

                row.CreateCell(2).SetCellValue(tn.Client);
                row.GetCell(2).CellStyle = cellStyle;

                //TODO:
                row.CreateCell(3).SetCellValue("КарТэк");
                row.GetCell(3).CellStyle = cellStyle;

                //TODO:
                row.CreateCell(4).SetCellValue(tn.Order.Service == ServiceType.Supply ? "Поставка" : "Перевозка");
                row.GetCell(4).CellStyle = cellStyle;

                row.CreateCell(5).SetCellValue(tn.Number);
                row.GetCell(5).CellStyle = cellStyle;

                row.CreateCell(6).SetCellValue(tn.LocationA);
                row.GetCell(6).CellStyle = cellStyle;

                row.CreateCell(7).SetCellValue(tn.LocationB);
                row.GetCell(7).CellStyle = cellStyle;

                row.CreateCell(8).SetCellValue(tn.CarPlate);
                row.GetCell(8).CellStyle = cellStyle;

                row.CreateCell(9).SetCellValue(tn.DriverInfo);
                row.GetCell(9).CellStyle = cellStyle;

                row.CreateCell(10).SetCellValue(StatusToString(tn.TaskStatus));
                row.GetCell(10).CellStyle = cellStyle;

                row.CreateCell(11).SetCellValue(tn.Material);
                row.GetCell(11).CellStyle = cellStyle;

                row.CreateCell(12).SetCellValue(tn.UnloadVolume);
                row.GetCell(12).CellStyle = cellStyle;

                row.CreateCell(13).SetCellValue(tn.UnloadUnit);
                row.GetCell(13).CellStyle = cellStyle;

                row.CreateCell(14).SetCellValue(tn.Order.Price.ToString());

                row.CreateCell(15).SetCellType(CellType.Formula);
                row.GetCell(15).SetCellFormula($"O{rowIndex + 1}*M{rowIndex + 1}");
                row.GetCell(15).CellStyle.WrapText = true;

                row.CreateCell(16).SetCellValue(tn.DriverPercent.ToString(nfi));

                row.CreateCell(17).SetCellType(CellType.Formula);

                if(tn.FixedPrice == null)
                {
                    row.GetCell(17).SetCellFormula($"Q{rowIndex + 1}*P{rowIndex + 1}/100");
                }
                else
                {
                    row.GetCell(17).SetCellFormula(tn.FixedPrice.Value.ToString(nfi));
                }

                row.CreateCell(18).SetCellValue(tn.IsVerified ? "Да" : "Нет");
                row.CreateCell(19).SetCellValue(tn.IsOriginalReceived ? "Да" : "Нет");

                rowIndex++;
            }

            var stream = new MemoryStream();

            for (var i = 0; i < 21; i++)
            {
                sheet.SetColumnWidth(i, 255 * 40);
            }

            var lst = tns.ToList();

            for (var i = 4; i < rowIndex; i++)
            {
                if (lst[i - 4].TaskStatus == DriverTaskStatus.Done)
                {
                    var row = sheet.GetRow(i);
                    var cell = row.GetCell(10);
                    var cellStyle2 = workbook.CreateCellStyle();

                    cellStyle2.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightGreen.Index;
                    cellStyle2.FillPattern = FillPattern.SolidForeground;

                    cellStyle2.BorderBottom = BorderStyle.Thin;
                    cellStyle2.BorderTop = BorderStyle.Thin;
                    cellStyle2.BorderLeft = BorderStyle.Thin;
                    cellStyle2.BorderRight = BorderStyle.Thin;

                    cell.CellStyle = cellStyle2;
                }
                else
                {
                    var row = sheet.GetRow(i);
                    var cell = row.GetCell(10);
                    var cellStyle2 = workbook.CreateCellStyle();

                    cellStyle2.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightYellow.Index;
                    cellStyle2.FillPattern = FillPattern.SolidForeground;

                    cellStyle2.BorderBottom = BorderStyle.Thin;
                    cellStyle2.BorderTop = BorderStyle.Thin;
                    cellStyle2.BorderLeft = BorderStyle.Thin;
                    cellStyle2.BorderRight = BorderStyle.Thin;

                    cell.CellStyle = cellStyle2;
                }
            }

            XSSFFormulaEvaluator.EvaluateAllFormulaCells(workbook);

            workbook.Write(stream, true);

            return stream;
        }
    }
}
