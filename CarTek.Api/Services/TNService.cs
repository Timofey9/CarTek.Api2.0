using AutoMapper;
using CarTek.Api.DBContext;
using CarTek.Api.Model;
using CarTek.Api.Model.Orders;
using CarTek.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq.Expressions;

namespace CarTek.Api.Services
{
    public class TNService : ITnService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;

        public TNService(ApplicationDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public IEnumerable<TNModel> GetAll(string? searchColumn, string? search, DateTime startDate, DateTime endDate)
        {
            return GetAllPagination("", "", 0, 0, searchColumn, search, startDate, endDate);
        }

        public IEnumerable<DriverSalaryTableModel> GetAllGrouped(string? searchColumn, string? search, DateTime startDate, DateTime endDate)
        {
            List<DriverSalaryTableModel> result = new List<DriverSalaryTableModel>();

            var grouped = GetAllPagination("", "", 0, 0, searchColumn, search, startDate, endDate).GroupBy(t => t.DriverInfo).ToList();

            foreach (var group in grouped)
            {
                double totalSalary = 0;

                foreach (var tn in group)
                {
                    totalSalary += GetTnDriverPrice(tn);
                }

                result.Add(new DriverSalaryTableModel
                {
                    Name = group.Key,
                    StartDate = startDate,
                    EndDate = endDate,  
                    TasksCount = group.Count(),
                    TotalSalary = totalSalary
                });
            }

            return result;
        }

        private double GetTnDriverPrice(TNModel tn)
        {
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ",";

            double volume;
            if (tn.Order.ReportLoadType == ReportLoadType.UseLoad)
            {
                double.TryParse(tn.LoadVolume, nfi, out volume);
            }
            else
            {
                double.TryParse(tn.UnloadVolume, nfi, out volume);
            }

            double totalSalary;
            if (tn.FixedPrice != null)
            {
                totalSalary = tn.FixedPrice.Value;
            }
            else
            {
                totalSalary = volume * tn.Order.DriverPrice * tn.DriverPercent / 100 ?? 0;
            }

            return totalSalary;
        }

        public IEnumerable<TNModel> GetAllPagination(string sortColumn, string sortDirection, int pageNumber, int pageSize, string searchColumn, string search, DateTime startDate, DateTime endDate)
        {
            var date1 = startDate.Date;
            var date2 = endDate.Date;

            var tnList = new List<TNModel>();
            Expression<Func<TN, bool>> filterBy;

            filterBy = x =>
                    (x.DriverTask != null || x.SubTask.DriverTask != null) &&
                    ((x.SubTask != null && x.SubTask.DriverTask != null
                    && !x.SubTask.IsCanceled
                    && !x.SubTask.DriverTask.IsCanceled
                    && x.SubTask.DriverTask.StartDate.Date >= date1 
                    && x.SubTask.DriverTask.StartDate.Date <= date2)
                    || (x.DriverTask != null 
                    && !x.DriverTask.IsCanceled 
                    && x.DriverTask.StartDate.Date >= date1 && x.DriverTask.StartDate.Date <= date2));

            if (search != null && !string.IsNullOrEmpty(searchColumn))
            {
                switch (searchColumn)
                {
                    case "tnNumber":
                        filterBy = x =>
                    (x.DriverTask != null || x.SubTask.DriverTask != null) 
                    && ((x.SubTask != null && x.SubTask.DriverTask != null
                    && !x.SubTask.IsCanceled
                    && !x.SubTask.DriverTask.IsCanceled
                    && x.SubTask.DriverTask.StartDate.Date >= date1 
                    && x.SubTask.DriverTask.StartDate.Date <= date2)
                    || (x.DriverTask != null
                    && !x.DriverTask.IsCanceled
                    && x.DriverTask.StartDate.Date >= date1 
                    && x.DriverTask.StartDate.Date <= date2))
                        && x.Number != null && x.Number.ToLower().Contains(search.ToLower());
                        break;
                    case "driver":
                        filterBy = x =>
                    (x.DriverTask != null || x.SubTask.DriverTask != null) 
                    && ((x.SubTask != null && x.SubTask.DriverTask != null
                    && !x.SubTask.IsCanceled
                    && !x.SubTask.DriverTask.IsCanceled
                    && x.SubTask.DriverTask.StartDate.Date >= date1 
                    && x.SubTask.DriverTask.StartDate.Date <= date2)
                    || (x.DriverTask != null
                    && !x.DriverTask.IsCanceled
                    && x.DriverTask.StartDate.Date >= date1 
                    && x.DriverTask.StartDate.Date <= date2))
                    && (x.DriverTask != null 
                    && x.DriverTask.Driver.LastName.ToLower().Contains(search.ToLower()) ||
                    x.SubTask != null && x.SubTask.DriverTask.Driver.LastName.ToLower().Contains(search.ToLower()));
                        break;
                    case "loadAddress":
                        filterBy = x =>
                    (x.DriverTask != null || x.SubTask.DriverTask != null) 
                    && ((x.SubTask != null && x.SubTask.DriverTask != null
                    && !x.SubTask.IsCanceled
                    && !x.SubTask.DriverTask.IsCanceled
                    && x.SubTask.DriverTask.StartDate.Date >= date1 
                    && x.SubTask.DriverTask.StartDate.Date <= date2) 
                    || (x.DriverTask != null
                    && !x.DriverTask.IsCanceled
                    && x.DriverTask.StartDate.Date >= date1 && x.DriverTask.StartDate.Date <= date2))
                    && x.LocationA != null && x.LocationA.TextAddress.ToLower().Contains(search.ToLower());
                        break;
                    case "unloadAddress":
                        filterBy = x =>
                    (x.DriverTask != null || x.SubTask.DriverTask != null) 
                    && ((x.SubTask != null && x.SubTask.DriverTask != null
                    && !x.SubTask.IsCanceled
                    && !x.SubTask.DriverTask.IsCanceled
                    && x.SubTask.DriverTask.StartDate.Date >= date1 
                    && x.SubTask.DriverTask.StartDate.Date <= date2)
                    || (x.DriverTask != null
                    && !x.DriverTask.IsCanceled
                    && x.DriverTask.StartDate.Date >= date1 
                    && x.DriverTask.StartDate.Date <= date2))                       
                    && x.LocationB != null && x.LocationB.TextAddress.ToLower().Contains(search.ToLower());
                        break;
                }
            }

            Expression<Func<TN, object>> orderBy = x => x.PickUpDepartureDate;

            var tresult = _dbContext.TNs
                .Include(tn => tn.SubTask)
                    .ThenInclude(st => st.DriverTask.Driver)
                .Include(tn => tn.SubTask)
                .Include(tn => tn.SubTask)
                    .ThenInclude(st => st.DriverTask)
                .Include(tn => tn.DriverTask)
                .Include(tn => tn.DriverTask)
                    .ThenInclude(dt => dt.Driver)
                 .Include(tn => tn.DriverTask)
                    .ThenInclude(dt => dt.Car)
                .Include(tn => tn.LocationA)
                .Include (tn => tn.LocationB)
                .Include(tn => tn.Material)
                .Include(tn => tn.Order)
                .Where(filterBy);

            tresult = tresult.OrderBy(orderBy);

            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ",";

            var result = new List<TNModel>();

            //Enumerate and finish request
            var list = tresult.ToList();

            foreach (var tn in list)
            {
                var tnModel = _mapper.Map<TNModel>(tn);
                double? fixedPrice = null;

                tnModel.OrderName = tn?.Order?.Name;
                tnModel.Material = tn.Material != null ? tn.Material.Name : "";

                if (tn.LocationA != null)
                {
                    tnModel.LocationA = tn.LocationA.TextAddress;
                }

                if (tn.LocationB != null)
                {
                    tnModel.LocationB = tn.LocationB.TextAddress;
                }

                if (tn.Order != null)
                {
                    //TODO: Должно быть из заявки
                    var customerId = tn.Order.Service == ServiceType.Transport ? tn.GoId : tn.GpId;
                    var customer = _dbContext.Clients.FirstOrDefault(t => t.Id == customerId);
                    tnModel.FixedPrice = customer?.FixedPrice;
                    tnModel.Customer = _mapper.Map<ClientModel>(customer);
                    tnModel.OrderId = tn.OrderId;
                }

                string carInfo = "";
                if (tn.DriverTask != null && tn.DriverTask.Driver != null)
                {
                    tnModel.DriverInfo = tn.DriverTask.Driver.FullName;
                    tnModel.TaskStatus = tn.DriverTask.Status;
                    tnModel.DriverPercent = tn.DriverTask.Driver.Percentage;
                    if (tn.DriverTask.Car != null)
                    {
                        carInfo = $"{tn.DriverTask.Car.Plate.ToUpper()} {tn.DriverTask.Car.Brand}";
                    }
                }

                if (tn.SubTask != null && tn.Order != null)
                {
                    //TODO: Должно быть из заявки
                    var customerId = tn.Order.Service == ServiceType.Transport ? tn.GoId : tn.GpId;
                    var customer = _dbContext.Clients.FirstOrDefault(t => t.Id == customerId);

                    tnModel.FixedPrice = customer?.FixedPrice;
                    tnModel.Customer = _mapper.Map<ClientModel>(customer);
                    tnModel.OrderId = tn.OrderId;
                    tnModel.TaskStatus = tn.SubTask.Status;

                    if (tn.SubTask.DriverTask != null)
                    {
                        carInfo = $"{tn.SubTask.DriverTask.Car.Plate.ToUpper()} {tn.SubTask.DriverTask.Car.Brand}";
                    }
                }

                if (tn.SubTask != null && tn.SubTask.DriverTask.Driver != null)
                {
                    tnModel.DriverInfo = tn.SubTask.DriverTask.Driver.FullName;
                    tnModel.DriverPercent = tn.SubTask.DriverTask.Driver.Percentage;
                }

                //не нужно дальше
                tnModel.Order.DriverTasks = null;

                double volume1 = tn.LoadVolume ?? 0;
                double volume2 = tn.UnloadVolume ?? 0;

                if (tn?.Order.LoadUnit == Unit.m3)
                {
                    volume1 = tn.LoadVolume ?? 0;
                    volume2 = tn.UnloadVolume ?? 0;
                }
                else
                {
                    volume1 = tn.LoadVolume2 ?? 0;
                    volume2 = tn.UnloadVolume2 ?? 0;
                }

                tnModel.Unit = UnitToString(tnModel.Order?.LoadUnit);
                tnModel.UnloadUnit = UnitToString(tnModel.Order?.LoadUnit);
                tnModel.LoadVolume = volume1.ToString(nfi);
                tnModel.UnloadVolume = volume2.ToString(nfi);
                tnModel.LoadVolume2 = volume1.ToString(nfi);
                tnModel.UnloadVolume2 = volume2.ToString(nfi); 
                tnModel.CarPlate = carInfo;                
                tnModel.PickUpDepartureTime = $"{tn.PickUpDepartureDate?.ToString("dd.MM.yyyy")}";
                tnModel.DropOffDepartureTime = $"{tn.DropOffDepartureDate?.ToString("dd.MM.yyyy")}";

                if (searchColumn == "customer" && search != null)
                {
                    if (tnModel.Customer != null && !string.IsNullOrEmpty(tnModel.Customer.ClientName) &&
                        tnModel.Customer.ClientName.ToLower().Contains(search.ToLower()))
                    {
                        result.Add(tnModel);
                    }
                }
                else
                {
                    result.Add(tnModel);
                }
            }

            if (pageSize > 0)
            {
                result = result.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            }

           
            return result;
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
    }
}
