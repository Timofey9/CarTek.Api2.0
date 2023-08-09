namespace CarTek.Api.Model.Orders
{
    public enum DriverTaskStatus
    {
        Assigned,
        Confirmed,
        Loading,
        Loaded,
        OnRoute,
        Unloading,
        Unloaded,
        DocsSent,
        OriginalReceived,
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

        public int Volume { get; set; }

        public Unit Unit { get; set; }

        public long OrderId { get; set; }
        
        public long DriverId { get; set; }
        
        public long CarId { get; set; }

        public DateTime LastUpdated { get; set; }

        public Car Car { get; set; }

        public Driver Driver { get; set; }

        public Order Order{ get; set; }

        public ICollection<DriverTaskNote> Notes { get; set; }
    }
}
