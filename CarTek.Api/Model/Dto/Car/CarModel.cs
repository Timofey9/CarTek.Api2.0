﻿using CarTek.Api.Model.Quetionary;

namespace CarTek.Api.Model.Dto
{
    public class CarModel
    {
        public long Id { get; set; }

        public string Plate { get; set; }

        public string Brand { get; set; }

        public string Model { get; set; }

        public int Mileage { get; set; }

        public int AxelsCount { get; set; }

        //В пути, на базе, в ремонте
        public string State { get; set; }

        public TrailerModel Trailer { get; set; }

        public DriverModel Driver { get; set; }
    }
}
