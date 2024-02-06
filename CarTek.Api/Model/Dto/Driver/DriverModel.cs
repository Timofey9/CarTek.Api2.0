using CarTek.Api.Model.Dto.Driver;

namespace CarTek.Api.Model.Dto
{
    public class DriverModel
    {
        public long Id { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public double Percentage { get; set; }

        public string LastName { get; set; }

        public string Password { get; set; }

        public string Phone { get; set; }

        public string Login { get; set; }

        public string FullName => $"{LastName} {FirstName} {MiddleName}";

        public DriverCarModel Car { get; set; }

        public long? CarId { get; set; }

        public string CarName { get; set; }

        public bool IsFired { get; set; }

        public bool IsExternal { get; set; }
        public long ExternalTransporterId { get; set; }
    }
}
