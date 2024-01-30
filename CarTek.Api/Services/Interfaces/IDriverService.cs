using CarTek.Api.Model.Dto;
using CarTek.Api.Model;
using Microsoft.AspNetCore.JsonPatch;
using CarTek.Api.Model.Response;
using CarTek.Api.Model.Orders;

namespace CarTek.Api.Services.Interfaces
{
    public interface IDriverService
    {
        public Driver GetByName(string lastname);

        public IEnumerable<Driver> GetAll(bool includeFired = false);

        public IEnumerable<Driver> GetAll(string searchColumn, string search, bool includeFired = false);

        public IEnumerable<Driver> GetAll(string sortColumn, string sortDirection, int pageNumber, int pageSize,
            string searchColumn, string search, bool includeFired = false);

        public ApiResponse CreateDriver(CreateDriverModel car);

        public Driver GetById(long carId);

        public ApiResponse DeleteDriver(long carId);

        public Driver UpdateDriver(long driverId, JsonPatchDocument<Driver> driverModel);

        public ApiResponse FireDriver(long driverId);

        public ICollection<Driver> GetExternalTransporterDrivers(long transporterId);
    }
}
