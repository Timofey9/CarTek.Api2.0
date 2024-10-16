﻿namespace CarTek.Api.Model.Dto.Driver
{
    public class DriverCarModel
    {
        public long Id { get; set; }

        public string Plate { get; set; }

        public string Brand { get; set; }

        public string Model { get; set; }

        public int Mileage { get; set; }

        public int AxelsCount { get; set; }

        //В пути, на базе, в ремонте
        public string State { get; set; }
        public bool IsExternal { get; set; }
    }
}
