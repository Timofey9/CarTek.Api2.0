using CarTek.Api.DBContext;
using CarTek.Api.Model;
using CarTek.Api.Model.Response;
using CarTek.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CarTek.Api.Services
{
    public class ClientService : IClientService
    {
        ApplicationDbContext _dbContext;
        ILogger<ClientService> _logger;

        public ClientService(ILogger<ClientService> logger, ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public ApiResponse CreateClient(string clientName, string inn, string clientAddress, Unit clientUnit, double? fixedPrice)
        {
            try
            {
                var client = new Client
                {
                    ClientAddress = clientAddress,
                    ClientName = clientName,
                    Inn = inn,
                    ClientUnit = clientUnit,
                    FixedPrice = fixedPrice
                };

                var hasClient = _dbContext.Clients.Any(t => t.ClientName.ToLower() == clientName.ToLower());

                if (!hasClient)
                {
                    var entity = _dbContext.Clients.Add(client);
                    _dbContext.SaveChanges();

                    return new ApiResponse
                    {
                        IsSuccess = true,
                        Message = $"Клиент {clientName} добавлен"
                    };
                }
                else
                {
                    return new ApiResponse
                    {
                        IsSuccess = false,
                        Message = "Клиент с таким именем уже существует!"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка создания клиента", ex);
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Ошибка создания клиента"
                };
            }
        }

        public ApiResponse DeleteClient(long id)
        {
            try
            {
                var client = _dbContext.Clients.Include(o => o.Orders).FirstOrDefault(c => c.Id == id);

                if (client != null)
                {
                    _dbContext.Clients.Remove(client);
                    _dbContext.SaveChanges();

                    return new ApiResponse
                    {
                        IsSuccess = true,
                        Message = $"Клиент {client.ClientName} удален"
                    };
                }
                else
                {
                    return new ApiResponse
                    {
                        IsSuccess = false,
                        Message = $"Клиент {client.ClientName} не найден"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка удаления клиента", ex);
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = $"Ошибка удаления клиента, {ex.Message}"
                };
            }
        }

        public Client GetClient(long id)
        {
            try
            {
                var client = _dbContext.Clients.FirstOrDefault(c => c.Id == id);

                return client;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка получения клиента: {id}", ex);
                return null;
            }
        }

        public ICollection<Client> GetClients()
        {
            var clients = _dbContext.Clients.OrderBy(t => t.ClientName);

            return clients.ToList();
        }

        public ApiResponse UpdateClient(long? id, string? clientName, string? inn, string? clientAddress, Unit clientUnit, double? fixedPrice)
        {
            if(id == null)
            {
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = $"Клиент не найден"
                };
            }

            var client = GetClient(id ?? 0);

            if (client != null)
            {
                if (clientName != null)
                {
                    client.ClientName = clientName;
                }

                if (inn != null)
                {
                    client.Inn = inn;
                }

                if (clientAddress != null)
                {
                    client.ClientAddress = clientAddress;
                }

                client.FixedPrice = fixedPrice;

                client.ClientUnit = clientUnit;

                _dbContext.Update(client);
                _dbContext.SaveChanges();
                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "Информация о клиенте обновлена"
                };
            }

            return new ApiResponse
            {
                IsSuccess = false,
                Message = $"Клиент {id} не найден"
            };
        }
    }
}
