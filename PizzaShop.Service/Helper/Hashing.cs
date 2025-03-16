using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace PizzaShop.Service.Helper;

public class Hashing
{
    public static bool VerifyPassword (string enterPassword, string storedPassword){
        string[] saltAndPassword = storedPassword.Split("::");

        byte[] saltbytes = Convert.FromBase64String(saltAndPassword[1]);

        string enterHashPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: enterPassword,
            salt: saltbytes,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 /8
        ));

        return enterHashPassword == saltAndPassword[0];
    }

    public static string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);

        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
             password: password!,
             salt: salt,
             prf: KeyDerivationPrf.HMACSHA256,
             iterationCount: 100000,
             numBytesRequested: 256 / 8
        ));

        string passWithsalt = string.Join("::",hashed,Convert.ToBase64String(salt));
        return passWithsalt;
    }
}
