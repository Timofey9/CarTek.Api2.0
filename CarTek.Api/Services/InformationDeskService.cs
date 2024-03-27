using CarTek.Api.DBContext;
using CarTek.Api.Model;
using CarTek.Api.Model.Response;
using CarTek.Api.Services.Interfaces;

namespace CarTek.Api.Services
{
    public class InformationDeskService : IInformationDeskService
    {
        private readonly ApplicationDbContext _dbContext;

        public InformationDeskService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public ApiResponse AddMessage(string message, InformationDeskMessageType type)
        {
            try
            {
                var messageObject = new InformationDeskMessage { Message = message, MessageType = type, DateCreated = DateTime.UtcNow };
                _dbContext.InformationDeskMessages.Add(messageObject);

                _dbContext.SaveChanges();
                return new ApiResponse
                {
                    Message = "Сообщение добавлено",
                    IsSuccess = true,
                };
            }
            catch {
                return new ApiResponse
                {
                    Message = "Сообщение не добавлено",
                    IsSuccess = false,
                };
            }
        }

        public ApiResponse DeleteMessage(long id)
        {
            var message = _dbContext.InformationDeskMessages.FirstOrDefault(t => t.Id == id);
            if(message != null)
            {
                _dbContext.InformationDeskMessages.Remove(message);
                _dbContext.SaveChanges();

                return new ApiResponse
                {
                    Message = "Сообщение удалено",
                    IsSuccess = true,
                };
            }
            return new ApiResponse
            {
                Message = "Сообщение не удалено",
                IsSuccess = false,
            };
        }

        public ICollection<InformationDeskMessage> GetMessages()
        {
            var messages = _dbContext.InformationDeskMessages.OrderByDescending(t => t.DateCreated);
            return messages.ToList();
        }
    }
}
