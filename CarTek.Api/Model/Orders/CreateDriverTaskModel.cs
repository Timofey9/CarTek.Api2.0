﻿using CarTek.Api.Model.Orders;

namespace CarTek.Api.Model
{
    public class CreateDriverTaskModel
    {
        public long OrderId { get; set; }

        public ShiftType Shift { get; set; }
        
        public long DriverId { get; set; }

        public long CarId { get; set; }

        public DateTime TaskDate { get; set; }

        public bool ForceChange { get; set; }

        public string? Comment { get; set; }
    }

    public class CreateDriverMultipleTaskModel
    {
        public long OrderId { get; set; }

        public ICollection<CreateDriverTaskModel> Tasks { get; set; }   
    }
}
