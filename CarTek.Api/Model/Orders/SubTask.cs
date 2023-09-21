namespace CarTek.Api.Model.Orders
{
    public class SubTask
    {
        public long Id { get; set; }

        public long DriverTaskId { get; set; }

        public long OrderId { get; set; }

        public int SequenceNumber { get; set; }  

        public DriverTaskStatus Status { get; set; }

        public TN? TN { get; set; }

        public ICollection<DriverTaskNote> Notes { get; set; }
    }
}
