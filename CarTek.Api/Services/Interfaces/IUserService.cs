using CarTek.Api.Model;
using CarTek.Api.Model.Dto;
using CarTek.Api.Model.Response;
using Microsoft.AspNetCore.JsonPatch;

namespace CarTek.Api.Services.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse> RegisterUser(CreateUserModel user);
        Task<ApiResponse> UpdateUser(string login, JsonPatchDocument<User> patchDoc);
        User DeleteUser(string login);
        User Get(UserAuthModel authModel);
        User GetByLogin(string login);

        public IEnumerable<User> GetAll();

        public IEnumerable<User> GetAll(string searchColumn, string search);

        public IEnumerable<User> GetAll(string sortColumn, string sortDirection, int pageNumber, int pageSize,
            string searchColumn, string search);

        UserAuthResult Authenticate(UserAuthModel authModel);

        public string GetHash(string input);
    }
}
