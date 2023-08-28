namespace CarTek.Api.Model
{
    public class UserDevice
    {
        public long Id { get; set; }

        public long UserId { get; set; }

        public bool IsDriver { get; set; }

        public string Token { get; set; }
    }
}
