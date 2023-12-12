namespace CarTek.Api.Model.Dto
{    
    #nullable disable
    public class CreateUserModel
    {
        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public string Login { get; set; }

        public string Email { get; set; }

        public bool IsAdmin { get; set; }

        public bool IsDispatcher { get; set; }

        //Бухгалтер первички
        public bool IsInitialBookkeeper { get; set; }

        //Бухгалтер ЗП
        public bool IsSalaryBookkeeper { get; set; }

        public string Phone { get; set; }

        public string Password { get; set; }

        public bool UserExists { get; set; }
    }
}
