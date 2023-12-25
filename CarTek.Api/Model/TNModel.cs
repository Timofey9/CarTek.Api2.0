using CarTek.Api.Model.Dto;
using CarTek.Api.Model.Orders;

namespace CarTek.Api.Model
{
    public class EditTNModel
    {
        public string Number { get; set; }

        public bool IsOriginalReceived { get; set; }

        public DateTime Date { get; set; }

        public string LoadVolume { get; set; }

        public string LoadVolume2 { get; set; }

        public string UnloadVolume { get; set; }

        public string UnloadVolume2 { get; set; }

        public string Unit { get; set; }

        public string Unit2 { get; set; }

        public string UnloadUnit { get; set; }

        public string UnloadUnit2 { get; set; }

        public string PickUpArrivalTime { get; set; }

        public string PickUpDepartureTime { get; set; }

        public string DropOffArrivalTime { get; set; }

        public string DropOffDepartureTime { get; set; }

        public string Transporter { get; set; }

        public ClientModel Gp { get; set; }

        public ClientModel Go { get; set; }

        public MaterialModel Material { get; set; }

        public string MaterialAmount { get; set; }

        public AddressModel LocationA { get; set; }

        public AddressModel LocationB { get; set; }
    }

    public class TNModel 
    {
        public long Id { get; set; }
        public bool IsVerified { get; set; }
        public bool IsOriginalReceived { get; set; }
        public string Number { get; set; }

        public DateTime Date { get; set; }

        public string LoadVolume { get; set; }

        public string LoadVolume2 { get; set; }

        public string UnloadVolume { get; set; }
        public string UnloadVolume2 { get; set; }

        public string Unit { get; set; }
        public string Unit2 { get; set; }

        public string UnloadUnit { get; set; }
        public string UnloadUnit2 { get; set; }

        public string PickUpArrivalTime { get; set; }
        public string PickUpDepartureTime { get; set; }

        public string DropOffArrivalTime { get; set; }
        public string DropOffDepartureTime { get; set; }

        //Название, юр. адрес, телефон
        public string GpInfo { get; set; }

        public ClientModel Gp { get; set; }

        public ClientModel Go { get; set; }

        public string Client { get; set; }

        public string GoInfo { get; set; }

        public string DriverInfo { get; set; }

        public string Transporter { get; set; }

        public string Material { get; set; }

        public string MaterialAmount { get; set; }  

        public string CarModel { get; set; }

        public string CarPlate { get; set; }

        public string TrailerPlate { get; set; }

        public string LocationA { get; set; }

        public string LocationB { get; set; }

        public DriverTaskStatus TaskStatus { get; set; }

        public double DriverPercent { get; set; }

        public List<string> S3Links { get; set; }

        public double? FixedPrice { get; set; }

        public Order Order { get; set; }

        public ClientModel Customer { get; set; }

        public long DriverTaskId { get;set; }
        public long SubTaskId { get; set; }
        public long OrderId { get; set; }
    }
}