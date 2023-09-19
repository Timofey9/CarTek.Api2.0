using CarTek.Api.Model.Orders;

namespace CarTek.Api.Model.Dto
{
    public class FillDocumentModel
    {
        public int UpdatedStatus { get; set; }

        public ICollection<IFormFile>? Files { get; set; }

        public string? Note { get; set; }

        public string? Number { get; set; }

        //чтобы понять, к чему привязана
        public long DriverTaskId { get; set; }

        //Id грузоотправителя из клиентов
        public long? GoId { get; set; }

        //Id грузополучателя из клиентов
        public long? GpId { get; set; }

        public double? LoadVolume { get;set; }

        public double? UnloadVolume { get; set; }

        public Unit? Unit { get; set; }

        //Id точки забора груза
        public int? LocationAId { get; set; }

        //Id точки сдачи груза
        public int? LocationBId { get; set; }

        public DateTime? PickUpDepartureDate { get; set; }
        public DateTime? PickUpArrivalDate { get; set; }

        public string? PickUpArrivalTime { get; set; }

        public string? PickUpDepartureTime { get; set; }

        public DateTime? DropOffArrivalDate { get; set; }

        public DateTime? DropOffDepartureDate { get; set; }

        public string? DropOffArrivalTime { get; set; }

        public string? DropOffDepartureTime { get; set; }
    }
}
