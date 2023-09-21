using CarTek.Api.Model.Orders;

namespace CarTek.Api.Model.Dto
{
    public class SubTaskModel
    {
        public long Id { get; set; }

        public long DriverTaskId { get; set; }

        public long OrderId { get; set; }

        public int SequenceNumber { get; set; }

        public DriverTaskStatus Status { get; set; }

        public TNModel? TN { get; set; }

        public ICollection<DriverTaskNote> Notes { get; set; }
    }

    public class CreateSubTaskModel
    {
        public long DriverTaskId { get; set; }
    }
}
