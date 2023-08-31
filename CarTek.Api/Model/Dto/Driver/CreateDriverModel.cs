namespace CarTek.Api.Model.Dto
{
    public class CreateDriverModel
    {
        public string FirstName { get; set; }

        public string? MiddleName { get; set; }

        public string LastName { get; set; }

        public string Login { get;set; }

        public string Password { get; set; }

        public long? CarId { get; set; }

        public string? Phone { get; set; }   
    }
}
