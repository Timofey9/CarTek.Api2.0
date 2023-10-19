using CarTek.Api.Model.Orders;

namespace CarTek.Api.Model
{
    public class TN
    {
        public long Id { get; set; }

        public string? Number { get; set; }

        //чтобы понять, к чему привязана
        public long? DriverTaskId { get; set; }

        public long? SubTaskId { get; set; }

        public long DriverId { get; set; }

        public long? TransporterId { get; set; } //Id из таблицы Clients, если null => КарТэк

        public double? LoadVolume { get; set; }
        public double? LoadVolume2 { get; set; }
        public double? UnloadVolume { get; set; }
        public double? UnloadVolume2 { get; set; }

        public Unit? Unit { get; set; }

        public Unit? Unit2 { get; set; }

        public Unit? UnloadUnit { get; set; }

        public Unit? UnloadUnit2 { get; set; }

        //Id грузоотправителя из клиентов
        public long? GoId { get; set; }

        //Id грузополучателя из клиентов
        public long? GpId { get; set; }

        //Id точки забора груза
        public int? LocationAId { get; set; }

        //Id точки сдачи груза
        public int? LocationBId { get; set; }

        public long? MaterialId { get; set; }

        public DateTime? PickUpArrivalDate { get; set; }

        public DateTime? PickUpDepartureDate { get; set; }

        public string? PickUpArrivalTime { get; set; }

        public string? PickUpDepartureTime { get; set; }

        public DateTime? DropOffArrivalDate { get; set; }

        public DateTime? DropOffDepartureDate { get; set; }

        public string? DropOffArrivalTime { get; set; }

        public string? DropOffDepartureTime { get; set; }

        public DriverTask? DriverTask { get; set; }

        public SubTask? SubTask { get; set; }

        public Material? Material { get; set; }
    }
}
