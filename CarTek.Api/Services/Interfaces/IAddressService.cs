﻿using CarTek.Api.Model;
using CarTek.Api.Model.Response;

namespace CarTek.Api.Services.Interfaces
{
    public interface IAddressService
    {
        Address GetAddress(long id);

        ApiResponse CreateAddress(string coordinates, string textAddress);

        ApiResponse DeleteAddress(long id);

        ApiResponse UpdateAddress(long? id, string? coordinates, string? textAddress);

        IEnumerable<Address> GetAddresses();
    }
}
