using CarTek.Api.Model.Orders;

namespace CarTek.Api.Model.Dto
{
    public class FillDocumentModel
    {
        public bool IsSubtask { get; set; }

        public long? SubTaskId { get; set; }

        public int UpdatedStatus { get; set; }

        public ICollection<IFormFile>? Files { get; set; }

        public string? Transporter { get; set; }
        public long? TransporterId { get; set; }

        public string? Note { get; set; }

        public string? Number { get; set; }

        public long MaterialId { get; set; }

        //чтобы понять, к чему привязана
        public long DriverTaskId { get; set; }

        //Id грузоотправителя из клиентов
        public long? GoId { get; set; }

        //Id грузополучателя из клиентов
        public long? GpId { get; set; }

        public double? LoadVolume { get;set; }

        public double? UnloadVolume { get; set; }

        public double? LoadVolume2 { get; set; }

        public double? UnloadVolume2 { get; set; }

        public Unit? Unit { get; set; }
        public Unit? Unit2 { get; set; }

        public Unit? UnloadUnit { get; set; }
        public Unit? UnloadUnit2 { get; set; }

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
