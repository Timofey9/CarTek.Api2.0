﻿using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CarTek.Api.Const
{
    public class AuthOptions
    {
        public const string ISSUER = "MyAuthServer"; // издатель токена
        public const string AUDIENCE = "MyAuthClient"; // потребитель токена
        const string KEY = "mysupersecret_secretkey!123";   // ключ для шифрации
        public const int LIFETIME = 100; // время жизни токена - 1 минута
        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }
    }

    public static class AuthPolicies
    {
        public const string ADMIN_ONLY = "ADMIN_ONLY";
        public const string DRIVER_ONLY = "DRIVER_ONLY";
    }
}
