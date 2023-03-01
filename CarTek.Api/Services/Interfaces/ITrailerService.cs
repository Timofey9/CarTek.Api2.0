using CarTek.Api.Model;
using CarTek.Api.Model.Response;
using Microsoft.AspNetCore.JsonPatch;

namespace CarTek.Api.Services.Interfaces
{
    public interface ITrailerService
    {
        public ApiResponse CreateTrailer(CreateTrailerModel model);
        public ApiResponse UpdateTrailer(long trailerId, JsonPatchDocument<Trailer> trailerModel);

        public Trailer GetByPlate(string plate);

        public IEnumerable<Trailer> GetAll();

        public IEnumerable<Trailer> GetAll(string searchColumn, string search);

        public IEnumerable<Trailer> GetAll(string sortColumn, string sortDirection, int pageNumber, int pageSize,
            string searchColumn, string search);
    }

    public class CreateTrailerModel
    {
        public string Plate { get; set; }

        public string Brand { get; set; }

        public string Model { get; set; }

        public long CarId { get; set; }

        public int AxelsCount { get; set; }
    }
}
