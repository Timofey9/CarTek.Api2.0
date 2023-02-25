namespace CarTek.Api.Model.Dto
{
    public class CreateDriverModel
    {
        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }   

        public bool DriverExists { get; set; }
    }
}
