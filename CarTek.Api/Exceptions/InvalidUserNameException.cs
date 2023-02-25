namespace CarTek.Api.Exceptions
{
    public class InvalidUsernameException : Exception
    {
        public InvalidUsernameException(string userName) : base($"Invalid username '{userName}' login attempt")
        {
        }
    }
}
