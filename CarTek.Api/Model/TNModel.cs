namespace CarTek.Api.Model
{
    public class TNModel
    {
        //Название, юр. адрес, телефон
        public string ClientInfo { get; set; }

        public string DriverInfo { get; set; }

        public string Sender { get; set; }

        public string Accepter { get; set; }

        public string Material { get; set; }

        public string MaterialAmount { get; set; }  

        public string CarModel { get; set; }

        public string CarPlate { get; set; }

        public string TrailerPlate { get; set; }

        public string LocationA { get; set; }

        public string LocationB { get; set; }
    }
}