using CarTek.Api.Model.Dto;
using CarTek.Api.Model.Quetionary;

namespace CarTek.Api.Model
{
    public class UnitQuestionaryModel
    {
        public Guid UniqueId { get; set; }

        public string Action { get; set; }

        public string Type { get; set; }

        public DateTime LastUpdated { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public string Comment { get; set; }

        public bool? WasApproved { get; set; }
        //Пробег
        public int? Mileage { get; set; }

        //Nav properties
        public CarModel Car { get; set; }
        public DriverModel Driver { get; set; }
        public UserModel User { get; set; }
        public TrailerModel Trailer { get; set; }


        public CarQuestionaryModel CarQuestionaryModel { get;set;}
        public TrailerQuestionaryModel TrailerQuestionaryModel { get;set;}
    }
}
