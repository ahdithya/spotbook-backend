using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using SpotBook.Domain.Entities;

namespace SpotBook.Application.Common.Interfaces
{
    public interface IJwtService

    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}