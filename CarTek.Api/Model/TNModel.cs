namespace CarTek.Api.Model
{
    public class TNModel
    {
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

        public string GoInfo { get; set; }

        public string DriverInfo { get; set; }

        public string Accepter { get; set; }

        public long? TransporterId { get; set; } //Id из таблицы Clients, если null => КарТэк

        public string Material { get; set; }

        public string MaterialAmount { get; set; }  

        public string CarModel { get; set; }

        public string CarPlate { get; set; }

        public string TrailerPlate { get; set; }

        public string LocationA { get; set; }

        public string LocationB { get; set; }

        public Order Order { get; set; }
    }
}