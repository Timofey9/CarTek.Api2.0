using CarTek.Api.DBContext;
using CarTek.Api.Model;
using CarTek.Api.Model.Dto;
using CarTek.Api.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch;
using System.Security.Claims;
using CarTek.Api.Const;
using CarTek.Api.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;
using CarTek.Api.Model.Response;
using Amazon.Runtime;

namespace CarTek.Api.Services
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IJwtService _jwtService;
        private readonly ApplicationDbContext _dbContext;
        private readonly IQuestionaryService _questionaryService;
        private readonly IReportGeneratorService _reportGeneratorService;
        public UserService(ILogger<UserService> logger, ApplicationDbContext dbContext, IQuestionaryService questionaryService, IJwtService jwtService, IReportGeneratorService reportGeneratorService)
        {
            _logger = logger;
            _jwtService = jwtService;
            _dbContext = dbContext;
            _questionaryService = questionaryService;
            _reportGeneratorService = reportGeneratorService;
        }

        public User DeleteUser(string login)
        {
            try
            {
                var user = _dbContext.Users.FirstOrDefault(t => t.Login.Equals(login));
                if (user == null)
                {
                    _logger.LogWarning($"Пользователь с логином {login} не существует");
                    return null;
                }

                var questionariesAssociated = _dbContext.Questionaries.Where(t => t.UserId == user.Id);

                foreach (var questionary in questionariesAssociated)
                {
                    _questionaryService.DeleteQuestionaryEntity(questionary);
                }

                _dbContext.Users.Remove(user);

                _dbContext.SaveChanges();

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка удаления пользователя {login}: {ex.Message}");
                return null;
            }
        }

        public IEnumerable<User> GetAll()
        {
            return GetAll(null, null, 0, 0, null, null);
        }

        public IEnumerable<User> GetAll(string sortColumn, string sortDirection, int pageNumber, int pageSize, string searchColumn, string search)
        {
            pageNumber = pageNumber > 0 ? pageNumber : 1;
            pageSize = pageSize >= 0 ? pageSize : 10;

            var result = new List<User>();

            try
            {
                Expression<Func<User, bool>> filterBy = x => true;
                if (!string.IsNullOrEmpty(searchColumn) && !string.IsNullOrEmpty(search))
                {
                    switch (searchColumn)
                    {
                        case "lastname":
                            filterBy = x => x.LastName.ToLower().Contains(search.ToLower().Trim());
                            break;
                        case "login":
                            filterBy = x => x.Login.ToLower().Contains(search.ToLower().Trim());
                            break;
                        default:
                            break;
                    }
                }

                Expression<Func<User, object>> orderBy = x => x.Id;

                if (sortColumn != null)
                {
                    switch (sortColumn)
                    {
                        case "login":
                            orderBy = x => x.Login;
                            break;
                        case "lastname":
                            orderBy = x => x.LastName;
                            break;
                        default:
                            break;
                    }
                }

                var tresult = _dbContext.Users.Where(filterBy);

                if (sortDirection == "asc")
                {
                    tresult = tresult.OrderBy(orderBy);
                }
                else
                {
                    tresult = tresult.OrderByDescending(orderBy);
                }

                if (pageSize > 0)
                {
                    tresult = tresult.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                }

                result = tresult.ToList();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось получить список пользователей");
            }

            return result;
        }

        public IEnumerable<User> GetAll(string searchColumn, string search)
        {
            return GetAll(null, null, 0, 0, searchColumn, search);
        }

        public User Get(UserAuthModel authModel)
        {
            string passwordHash = GetHash(authModel.Password);

            var userInstance = _dbContext.Users
                .FirstOrDefault(u => u.Login.ToLower() == authModel.Login.Trim().ToLower());

            if (userInstance == null)
            {
                throw new InvalidUsernameException(authModel.Login);
            }

            if (userInstance.Password != passwordHash)
            {
                throw new InvalidPasswordException();
            }
            return userInstance;        
        }


        private Driver GetDriver(UserAuthModel authModel)
        {
            var driverInstance = _dbContext.Drivers
                .FirstOrDefault(u => u.Login.ToLower() == authModel.Login.Trim().ToLower());

            if (driverInstance == null)
            {
                throw new InvalidUsernameException(authModel.Login);
            }

            if (driverInstance.Password != authModel.Password.Trim())
            {
                throw new InvalidPasswordException();
            }

            return driverInstance;
        }

        public string GetHash(string input)

        {
            using (SHA256 hashFunction = SHA256.Create())
            {
                byte[] inputBytes = new ASCIIEncoding().GetBytes(input);

                byte[] hashInput = hashFunction.ComputeHash(inputBytes);

                String.Concat(Array.ConvertAll(hashInput, x => x.ToString("x2")));

                return Convert.ToBase64String(hashInput);
            }
        }

        public async Task<ApiResponse> RegisterUser(CreateUserModel user)
        {
            try
            {
                var dbUser = _dbContext.Users.FirstOrDefault(t => t.Login.Equals(user.Login));

                if (dbUser != null)
                {
                    var message = $"Пользователь с логином {user.Login} уже существует";
                    _logger.LogWarning(message);
                    return new ApiResponse { IsSuccess = false, Message = message };
                }

                var newUser = new User
                {
                    FirstName = user.FirstName,
                    MiddleName = user.MiddleName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Login = user.Login,
                    Phone = user.Phone,
                    IsAdmin = user.IsAdmin,
                    IsDispatcher = user.IsDispatcher,
                    IsInitialBookkeeper = user.IsInitialBookkeeper,
                    IsSalaryBookkeeper = user.IsSalaryBookkeeper,
                    IsLogistManager = user.IsLogistManager,
                    Password = GetHash(user.Password)
                };

                _dbContext.Users.Add(newUser);

                await _dbContext.SaveChangesAsync();

                return new ApiResponse { IsSuccess = true, Message = $"Пользователь {user.Login} создан" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Не удалось создать пользователя: {ex.Message}");
                return new ApiResponse { IsSuccess = false, Message = $"Не удалось создать пользователя: {ex.Message}" };
            }
        }

        public async Task<ApiResponse> UpdateUser(string login, [FromBody] JsonPatchDocument<User> patchDoc)
        {
            try
            {
                var existing = _dbContext.Users
                    .FirstOrDefault(u => u.Login.Equals(login));

                if (existing == null)
                {
                    return null;
                }

                patchDoc.Operations.Where(x => x.path.Equals("/password"))
                    .ToList()
                    .ForEach(x => x.value = GetHash(x.value.ToString()));

                patchDoc.ApplyTo(existing);

                existing.RefreshToken = null;

                _dbContext.Users.Update(existing);

                var modifiedEntries = _dbContext.ChangeTracker
                       .Entries()
                       .Where(x => x.State == EntityState.Added || x.State == EntityState.Modified || x.State == EntityState.Deleted || x.State == EntityState.Detached)
                       .Select(x => $"{x.DebugView.LongView}.\nState: {x.State}")
                       .ToList();

                await _dbContext.SaveChangesAsync();

                return new ApiResponse { IsSuccess = true, Message = "Пользователь успешно изменен" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Не удалось изменить пользователя {login}, {ex.Message}");
                return new ApiResponse { IsSuccess = false, Message = $"Не удалось изменить пользователя {login}" };
            }
        }

        public UserAuthResult Authenticate(UserAuthModel authModel)
        {
            try
            {
                Claim[] claims;
                User userInstance;
                string token;
                string refreshToken;
                bool isExternal = false;

                if (authModel.IsDriver == true)
                {
                    var driver = GetDriver(authModel);

                    if (driver.IsFired)
                    {
                        throw new InvalidPasswordException();
                    }

                    claims = new[]
                    {
                        new Claim(AuthConstants.ClaimTypeLogin, driver.Login),
                        new Claim(AuthConstants.ClaimTypeIsDriver, "True"),
                        new Claim(AuthConstants.ClaimTypeId, driver.Id.ToString())
                    };

                    token = _jwtService.GenerateToken(claims, 0, 1);

                    userInstance = new User
                    {
                        Id = driver.Id,
                        Login = driver.Login,
                        FirstName = driver.FirstName,
                        LastName = driver.LastName,
                        MiddleName = driver.MiddleName                        
                    };

                    isExternal = driver.IsExternal;

                    refreshToken = _jwtService.GenerateRefreshToken();

                    driver.RefreshToken = refreshToken;
                    driver.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(30);

                    _dbContext.Drivers.Update(driver);
                }
                else
                {
                    userInstance = Get(authModel);
                    claims = new[]
                    {
                        new Claim(AuthConstants.ClaimTypeLogin, userInstance.Login),
                        new Claim(AuthConstants.ClaimTypeIsAdmin, userInstance.IsAdmin.ToString()),                                                
                        new Claim(AuthConstants.ClaimTypeId, userInstance.Id.ToString())
                    };

                    token = _jwtService.GenerateToken(claims, 0, 1);
                    refreshToken = _jwtService.GenerateRefreshToken();

                    userInstance.RefreshToken = refreshToken;
                    userInstance.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

                    _dbContext.Users.Update(userInstance);
                }

                _dbContext.SaveChanges();

                return new UserAuthResult
                {
                    Token = token,
                    Identity = userInstance,
                    RefreshToken = refreshToken,
                    IsDriver = authModel.IsDriver ?? false,
                    IsExternal = isExternal
                };
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        public User GetByLogin(string login)
        {
            try
            {
                var user = _dbContext.Users.FirstOrDefault(t => t.Login.Equals(login));
                if (user == null)
                {
                    _logger.LogWarning($"Пользователь с логином {login} не существует");
                    return null;
                }
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка получения пользователя {login}: {ex.Message}");
                return null;
            }
        }
    }
}
