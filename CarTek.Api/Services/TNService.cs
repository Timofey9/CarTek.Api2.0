using AutoMapper;
using CarTek.Api.DBContext;
using CarTek.Api.Model;
using CarTek.Api.Model.Orders;
using CarTek.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
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
            return GetAllPagination("", "", 0, 0, searchColumn, search, startDate,endDate);
        }

        public IEnumerable<TNModel> GetAllPagination(string sortColumn, string sortDirection, int pageNumber, int pageSize, string searchColumn, string search, DateTime startDate, DateTime endDate)
        {
            var date1 = startDate.Date;
            var date2 = endDate.Date;

            var tnList = new List<TNModel>();
            Expression<Func<TN, bool>> filterBy;

            filterBy = x =>
                x.PickUpDepartureDate != null && x.DropOffDepartureDate != null &&
                 (x.PickUpDepartureDate.Value.Date >= date1
                && x.DropOffDepartureDate.Value.Date <= date2);

            if (search != null && !string.IsNullOrEmpty(searchColumn))
            {
                switch (searchColumn)
                {
                    case "tnNumber":
                        filterBy = x =>
                        x.PickUpDepartureDate != null && x.DropOffDepartureDate != null
                        && (x.PickUpDepartureDate.Value.Date >= date1
                        && x.DropOffDepartureDate.Value.Date <= date2)
                        && x.Number != null && x.Number.ToLower().Contains(search.ToLower());
                        break;
                }
            }

            Expression<Func<TN, object>> orderBy = x => x.PickUpDepartureDate;

            var tresult = _dbContext.TNs
                .Include(tn => tn.SubTask)
                .Include(tn => tn.DriverTask)
                    .ThenInclude(dt => dt.Car)
                .Include(tn => tn.DriverTask)
                    .ThenInclude(dt => dt.Order)
                .Include(tn => tn.Material)
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

                tnModel.Material = tn.Material != null ? tn.Material.Name : "";

                var locationA = _dbContext.Addresses.FirstOrDefault(t => t.Id == tn.LocationAId);
                if (locationA != null)
                {
                    tnModel.LocationA = locationA.TextAddress;
                }

                var locationB = _dbContext.Addresses.FirstOrDefault(t => t.Id == tn.LocationBId);
                if (locationB != null)
                {
                    tnModel.LocationB = locationB.TextAddress;
                }

                if(tn.DriverTask != null && tn.DriverTask.Order != null)
                {
                    var customerId = tn.DriverTask.Order.Service == ServiceType.Transport ? tn.GoId : tn.GpId;
                    var customer = _dbContext.Clients.FirstOrDefault(t => t.Id == customerId);
                    tnModel.Customer = _mapper.Map<ClientModel>(customer);
                    tnModel.OrderId = tn.DriverTask.OrderId;
                }

                var driver = _dbContext.Drivers.FirstOrDefault(t => t.Id == tn.DriverId);
                if (driver != null)
                {
                    tnModel.DriverInfo = driver.FullName;
                }

                if (searchColumn == "customer" && search != null)
                {
                    if (tnModel.Customer.ClientName.ToLower().Contains(search.ToLower()))
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
    }
}
