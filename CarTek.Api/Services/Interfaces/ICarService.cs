﻿using CarTek.Api.Model;
using CarTek.Api.Model.Dto;
using CarTek.Api.Model.Dto.Car;
using CarTek.Api.Model.Response;
using Microsoft.AspNetCore.JsonPatch;

namespace CarTek.Api.Services.Interfaces
{
    public interface ICarService
    {
        public Car GetByPlate(string plate);

        public IEnumerable<CarDriverTaskModel> GetCarsWithTasks(DateTime date);

        public IEnumerable<Car> GetAll();

        public IEnumerable<Car> GetAll(string searchColumn, string search);

        public IEnumerable<Car> GetAll(string sortColumn, string sortDirection, int pageNumber, int pageSize,
            string searchColumn, string search);

        public ApiResponse CreateCar(CreateCarModel car);

        public Car GetById(long carId);

        public ApiResponse DeleteCar(long carId);

        public Car UpdateCar(long carId, JsonPatchDocument<Car> carModel);

        public ICollection<Car> GetExternalTransporterCars(long transporterId);
    }
}
