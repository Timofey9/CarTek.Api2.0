using CarTek.Api.Model.Dto.Car;

namespace CarTek.Api.Model.Quetionary
{

    public class TransportQuestionaryModel
    {
        public long TransportId { get; set; }

        public bool GeneralCondition { get; set; }

        public WheelsJson WheelsJsonObject { get; set; }

        public LightsJsonObject LightsJsonObject { get; set; }

        public bool FendersOk{ get; set; }

        public bool FendersMountState { get; set; }
    }


    public class TrailerQuestionaryModel : TransportQuestionaryModel
    {
        public string TrailerComment { get; set; }  
    }


    public class CarQuestionaryModel : TransportQuestionaryModel
    {
        public int? Mileage { get; set; }

        public bool? IsCabinClean { get; set; }

        public bool PlatonInPlace { get; set; }

        public bool PlatonSwitchedOn { get; set; }

        public bool CabinCushion { get; set; }

        public bool Rack { get; set; }

        public bool FrontSuspension { get; set; }

        public bool BackSuspension { get; set; }

        public bool HydroEq { get; set; }
    }


    public class CreateQuestionaryModel
    {
        //User login
        public string CreatedBy { get; set; }

        public long Id { get; set; }

        public string CarQuestionaryModel { get; set; }

        public string TrailerQuestionaryModel { get; set; }

        public string? Comment { get; set; }

        public bool? WasApproved { get; set; }

        public ICollection<IFormFile>? Images { get; set; }

        public long? CarId { get; set; }

        public long? TrailerId { get; set; }

        public long DriverId { get; set; }
    }
}
