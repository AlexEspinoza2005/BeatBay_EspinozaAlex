using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BeatBay.Model;
using Microsoft.AspNetCore.Identity;

namespace BeatBay.Services
{
    public interface IJwtService
    {
        Task<string> GenerateTokenAsync(User user);
        ClaimsPrincipal? ValidateToken(string token);
        string GenerateRefreshToken();
    }
}