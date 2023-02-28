using CarTek.Api.Model;
using CarTek.Api.Model.Dto;
using Microsoft.AspNetCore.JsonPatch;

namespace CarTek.Api.Services.Interfaces
{
    public interface ICarService
    {
        public Car GetByPlate(string plate);

        public IEnumerable<Car> GetAll();

        public IEnumerable<Car> GetAllWithoutDriver();

        public IEnumerable<Car> GetAll(string searchColumn, string search);

        public IEnumerable<Car> GetAll(string sortColumn, string sortDirection, int pageNumber, int pageSize,
            string searchColumn, string search);

        public Car CreateCar(CreateCarModel car);

        public Car GetById(long carId);

        public Car DeleteCar(long carId);

        public Car UpdateCar(long carId, JsonPatchDocument<Car> carModel);
    }
}
