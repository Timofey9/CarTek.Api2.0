using CarTek.Api.Model.Dto;
using CarTek.Api.Model;
using Microsoft.AspNetCore.JsonPatch;

namespace CarTek.Api.Services.Interfaces
{
    public interface IDriverService
    {
        public Driver GetByName(string lastname);

        public IEnumerable<Driver> GetAll();

        public IEnumerable<Driver> GetAll(string searchColumn, string search);

        public IEnumerable<Driver> GetAll(string sortColumn, string sortDirection, int pageNumber, int pageSize,
            string searchColumn, string search);

        public Driver CreateDriver(CreateDriverModel car);

        public Driver GetById(long carId);

        public Driver DeleteDriver(long carId);

        public Driver UpdateDriver(long driverId, JsonPatchDocument<Driver> driverModel);
    }
}
