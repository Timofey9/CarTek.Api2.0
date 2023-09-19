﻿using CarTek.Api.DBContext;
using CarTek.Api.Model;
using CarTek.Api.Model.Response;
using CarTek.Api.Services.Interfaces;

namespace CarTek.Api.Services
{
    public class AddressService : IAddressService
    {
        private readonly ApplicationDbContext _dbContext;
        private ILogger<AddressService> _logger;

        public AddressService(ILogger<AddressService> logger, ApplicationDbContext applicationDbContext)
        {
            _dbContext = applicationDbContext;
            _logger = logger;
        }

        public ApiResponse CreateAddress(string coordinates, string textAddress)
        {
            try
            {
                var address = new Address {Coordinates = coordinates, TextAddress = textAddress };
                _dbContext.Addresses.Add(address);
                _dbContext.SaveChanges();
                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "Адрес добавлен"
                };
            }
            catch(Exception ex)
            {
                _logger.LogError("Адрес не добавлен", ex);
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Адрес не добавлен"
                };
            }
        }

        public ApiResponse DeleteAddress(long id)
        {
            try
            {
                var address = _dbContext.Addresses.FirstOrDefault(a => a.Id == id); 
                if(address != null)
                {
                    _dbContext.Addresses.Remove(address); 
                    _dbContext.SaveChanges();

                    return new ApiResponse
                    {
                        IsSuccess = true,
                        Message = "Адрес удален"
                    };
                }
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Адрес не найден"
                };
            }
            catch(Exception ex)
            {
                _logger.LogError("Адрес не удален", ex);
                return new ApiResponse
                {
                    IsSuccess = false,
                    Message = "Адрес не удален"
                };
            }
        }

        public Address GetAddress(long id)
        {
            var address = _dbContext.Addresses.FirstOrDefault(t => t.Id == id);

            return address;
        }

        public IEnumerable<Address> GetAddresses()
        {
            var addresses = _dbContext.Addresses.OrderBy(t => t.TextAddress);

            return addresses;
        }

        public ApiResponse UpdateAddress(long id, string? coordinates, string? textAddress)
        {
            var address = GetAddress(id);

            if (address != null)
            {
                if (!string.IsNullOrEmpty(coordinates))
                {
                    address.Coordinates = coordinates;
                }
                if (!string.IsNullOrEmpty(textAddress))
                {
                    address.TextAddress = textAddress;
                }
                _dbContext.Update(address);
                _dbContext.SaveChanges();

                return new ApiResponse
                {
                    IsSuccess = true,
                    Message = "Адрес обновлен"
                };
            }

            return new ApiResponse
            {
                IsSuccess = false,
                Message = $"Адрес {id} не найден"
            };
        }
    }
}
