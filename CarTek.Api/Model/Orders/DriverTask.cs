namespace CarTek.Api.Model.Orders
{
    public enum DriverTaskStatus
    {
        Assigned,
        Confirmed,
        OnRoute,
        Loading,
        DocumentSigning1,
        Loaded,
        OutLoad,
        ArrivedToUnload,
        Unloading,
        DocumentSigning2,
        Done
    }

    public enum ShiftType
    {
        Night = 0,
        Day = 1
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

        public int SubTasksCount { get; set; }

        public TN? TN { get; set; }

        public Car Car { get; set; }

        public Driver Driver { get; set; }

        public Order Order{ get; set; }

        public ICollection<DriverTaskNote> Notes { get; set; }

        public ICollection<SubTask> SubTasks { get; set; }
    }
}
