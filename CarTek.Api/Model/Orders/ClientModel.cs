﻿namespace CarTek.Api.Model.Orders
{
    public class ClientModel
    {
        public long? Id { get; set; }
        
        public string ClientName { get; set; }

        public string Inn { get; set; }

        public Unit ClientUnit { get; set; }

        public double? FixedPrice { get; set; }

        //public double? Density { get; set; }

        //public string Ogrn { get; set; }

        //public string Kpp { get; set; }

        public string ClientAddress { get; set; }
    }
}
