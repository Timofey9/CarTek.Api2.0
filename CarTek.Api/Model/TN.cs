using CarTek.Api.Model.Orders;

namespace CarTek.Api.Model
{
    public class TN
    {
        public long Id { get; set; }

        public long DriverTaskId { get; set; }

        public long GoId { get; set; }

        public long GpId { get; set; }

        public int LocationAId { get; set; }

        public int LocationBId { get; set; }

        public DriverTask DriverTask { get; set; }
    }
}
