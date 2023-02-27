namespace CarTek.Api.Model
{    

    public enum Condition
    {
        Ok, 
        NotOk
    }

    public class Questionary
    {
        public long Id { get; set; }

        //car or trailer
        public string Type { get; set; }

        //по приезду или отъезду
        public string Action { get; set; }

        //Id для хранения изображений + связующие элемент для осмотра тягача + прицепа
        public Guid UniqueId { get; set; }

        public bool GeneralCondition { get; set; }

        public string? ImagesPath { get; set; }

        public DateTime LastUpdated { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public string Comment { get; set; }

        public bool? WasApproved { get; set; }
        
        public int? Mileage { get; set; }

        public bool? IsCabinClean { get; set; }

        public bool PlatonInPlace { get; set; }

        public bool PlatonSwitchedOn { get; set; }  

        public string WheelsJsonObject { get; set; }

        public string LightsJsonObject { get; set; }

        public bool CabinCushion { get; set; }

        public string FendersJsonObject { get; set; }

        public bool Rack { get; set; }

        public bool FrontSuspension { get; set; }

        public bool BackSuspension { get; set; }

        public bool HydroEq { get; set; }


        //Nav properties
        public long? CarId { get; set; }
        public Car? Car { get; set; }    

        public long DriverId { get; set; }
        public Driver Driver { get; set; }

        public long UserId { get; set; }
        public User User { get; set; }

        public long? TrailerId { get; set; } 
        public Trailer? Trailer { get; set; }
    }
}
