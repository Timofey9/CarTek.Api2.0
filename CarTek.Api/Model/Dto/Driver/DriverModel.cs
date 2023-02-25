namespace CarTek.Api.Model.Dto
{
    public class DriverModel
    {
        public long Id { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string FullName => $"{FirstName} {MiddleName} {LastName}";

        public long? CarId { get; set; }

        public string CarName { get; set; }
    }
}
