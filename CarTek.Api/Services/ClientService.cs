using CarTek.Api.DBContext;
using CarTek.Api.Model;
using CarTek.Api.Model.Response;
using CarTek.Api.Services.Interfaces;

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

        public ApiResponse CreateClient(string clientName, string inn, string clientAddress)
        {
            try
            {
                var client = new Client
                {
                    ClientAddress = clientAddress,
                    ClientName = clientName,
                    Inn = inn
                };

                var entity = _dbContext.Clients.Add(client);

                _dbContext.SaveChanges();

                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = $"Клиент {clientName} добавлен"
                };
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
                var client = _dbContext.Clients.FirstOrDefault(c => c.Id == id);

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

        public ApiResponse UpdateClient(long id, string? clientName, string? inn, string? clientAddress)
        {
            var client = GetClient(id);

            if (client != null)
            {
                if (!string.IsNullOrEmpty(clientName))
                {
                    client.ClientName = clientName;
                }
                if (!string.IsNullOrEmpty(inn))
                {
                    client.Inn = inn;
                }
                //if (!string.IsNullOrEmpty(ogrn))
                //{
                //    client.Ogrn = ogrn;
                //}
                //if (!string.IsNullOrEmpty(kpp))
                //{
                //    client.Kpp = kpp;
                //}
                if (!string.IsNullOrEmpty(clientAddress))
                {
                    client.ClientAddress = clientAddress;
                }

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
