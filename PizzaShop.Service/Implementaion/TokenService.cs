using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PizzaShop.Entity.Models;

using PizzaShop.Service.Interface;

namespace PizzaShop.Service.Implementaion;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateAuthToken(User user, TimeSpan expiration)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.UserRole?.Name!),
            new Claim(ClaimTypes.Name,user.UserName),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        return GenerateToken(claims, expiration); 
    }

    public string GenerateResetToken(string email)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Email, email)
        };

        return GenerateToken(claims, TimeSpan.FromMinutes(30)); // 30 MIN EXPIRE TIME FOR RESET LINK
    }

    public string? ValidateResetToken(string token)
    {
        var dummyKey = _config["JwtSettings:Key"];
        var key = Encoding.UTF8.GetBytes(dummyKey!);
        var tokenhandler = new JwtSecurityTokenHandler();
        try 
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true 
            };
            var principal = tokenhandler.ValidateToken(token,validationParameters, out _);
            var emailClaim = principal.FindFirst(ClaimTypes.Email);

            return emailClaim.Value;
        }
        catch
        {
            return null;
        }   
    }
    
    private string GenerateToken(Claim[] claims, TimeSpan expiration)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _config["JwtSettings:Issuer"],
            audience: _config["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.Add(expiration),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }

}
