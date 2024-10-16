﻿namespace CarTek.Api.Model.Orders
{
    public enum DriverTaskStatus
    {
        Assigned = 0,
        Confirmed = 1,
        OnRoute = 2,
        Loading = 3,
        DocumentSigning1 = 4,
        OutLoad = 5,
        ArrivedToUnload = 6,
        Unloading = 7,
        DocumentSigning2 = 8,
        Done = 9,
        Canceled = 10
    }

    public enum ShiftType
    {
        Night = 0,
        Day = 1,
        Fullday = 2,
        Unlimited = 3 
    }

    public class DriverTask
    {
        public long Id { get; set; }
     
        public Guid UniqueId { get; set; }

        // day/night
        public ShiftType Shift { get; set; }

        public DriverTaskStatus Status { get; set; }

        public DateTime StartDate { get; set; }

        public double Volume { get; set; }

        public Unit Unit { get; set; }

        public long OrderId { get; set; }
        
        public long DriverId { get; set; }
        
        public long CarId { get; set; }

        public DateTime LastUpdated { get; set; }

        public DateTime DateCreated { get; set; }

        public string? AdminComment { get; set; }

        public bool IsCanceled { get; set; }

        public int SubTasksCount { get; set; }

        public TN? TN { get; set; }

        public Car Car { get; set; }

        public Driver Driver { get; set; }

        public Order Order{ get; set; }

        public ICollection<DriverTaskNote> Notes { get; set; }

        public ICollection<SubTask> SubTasks { get; set; }
    }
}
