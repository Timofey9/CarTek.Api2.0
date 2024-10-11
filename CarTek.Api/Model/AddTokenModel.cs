namespace CarTek.Api.Model
{
    public class AddTokenModel
    {
        public long UserId { get; set; }

        public string Token { get; set; }

        public bool IsDriver { get; set; }
    }
}
