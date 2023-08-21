namespace CarTek.Api.Model.Orders
{
    public class DriverTaskNote
    {
        public long Id { get; set; }

        public string Text { get; set; }

        public DriverTaskStatus Status { get; set; }

        public DateTime DateCreated { get; set; }

        public string S3Links { get; set; }

        public long DriverTaskId { get; set; }
    }
}
