namespace CarTek.Api.Model.Dto
{
    #nullable disable
    public class CreateCarModel
    {
        public string Plate { get; set; }

        public string Brand { get; set; }

        public string Model { get; set; }

        public long TrailerId { get; set; } 

        public int AxelsCount { get; set; }

        ////////////////        
        public bool IsExternal { get; set; }
        public long? ExternalTransporterId { get; set; }
    }
}
