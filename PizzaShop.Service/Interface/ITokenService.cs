using PizzaShop.Entity.Models;


namespace PizzaShop.Service.Interface;

public interface ITokenService
{
    string GenerateAuthToken(User user, TimeSpan expiration);
    

    string GenerateResetToken(string email);

    string? ValidateResetToken(string token);

}
