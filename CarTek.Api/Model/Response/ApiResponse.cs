namespace CarTek.Api.Model.Response
{
    public class ApiResponse
    {
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
    }

    public class ApiResponseResult<T> where T : class
    {
        public T Result { get; set; }
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
    }
}
