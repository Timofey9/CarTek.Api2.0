using CarTek.Api.Model.Orders;

namespace CarTek.Api.Services
{
    public class DriverTaskReportModel
    {
        public string Plate { get; set; }
        public string Driver { get; set; }
        public string Service { get; set; }
        public string Go { get; set; }
        public string Gp { get; set; }
        public string Client { get; set; }
        public string LocationA { get; set; }
        public string LocationB { get; set; }
        public string Material { get; set; }
        public ShiftType Shift { get; set; }
        public DriverTaskStatus Status { get; set; }
        public bool IsCanceled { get; set; }

        public string OrderComment { get; set; }
        public string TaskComment { get; set; }
    }
}