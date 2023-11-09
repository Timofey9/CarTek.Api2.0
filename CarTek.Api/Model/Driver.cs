using CarTek.Api.Model.Orders;

namespace CarTek.Api.Model
{
    public class Driver
    {
        public long Id { get; set; }

        public string Login { get; set; }

        public string FirstName { get; set; }

        public string? MiddleName { get; set; }

        public string LastName { get; set; }

        public string Password { get; set; }

        public string? Phone { get; set; }

        public double Percentage { get; set; }

        public ICollection<Questionary> Questionaries { get; set; }

        public ICollection<DriverTask> DriverTasks { get; set; }

        public long? CarId { get; set; }

        public Car? Car { get; set; }

        public string? RefreshToken { get; set; }

        public DateTime RefreshTokenExpiryTime { get; set; }

        public string FullName => $"{LastName} {FirstName} {MiddleName}";
    }
}
