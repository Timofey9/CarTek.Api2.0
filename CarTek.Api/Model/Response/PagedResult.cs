namespace CarTek.Api.Model.Response
{
    public class PagedResult<T>
    {
        public int TotalNumber;
        public List<T> List;
    }
}
