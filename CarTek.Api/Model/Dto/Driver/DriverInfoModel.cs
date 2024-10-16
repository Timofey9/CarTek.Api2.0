﻿namespace CarTek.Api.Model.Dto.Driver
{
    public class DriverInfoModel
    {
        public long Id { get; set; }
        public double Percentage { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public string Password { get; set; }

        public string Phone { get; set; }

        public bool IsExternal { get; set; }

        public string FullName => $"{LastName} {FirstName} {MiddleName}";
    }
}
