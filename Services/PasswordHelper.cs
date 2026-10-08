using System.Security.Cryptography;
using System.Text;

namespace WebApplication1.Services
{
    public static class PasswordHelper
    {
        public static byte[] Hash(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100000, HashAlgorithmName.SHA256, 32);

            byte[] result = new byte[48];
            Array.Copy(salt, 0, result, 0, 16);
            Array.Copy(hash, 0, result, 16, 32);
            return result;
        }

        public static bool Verify(string password, byte[] stored)
        {
            if (stored.Length == 32)
            {
                byte[] old = SHA256.HashData(Encoding.UTF8.GetBytes(password));
                return CryptographicOperations.FixedTimeEquals(old, stored);
            }

            if (stored.Length != 48) return false;

            byte[] salt = stored[..16];
            byte[] expected = stored[16..];
            byte[] actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100000, HashAlgorithmName.SHA256, 32);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
    }
}
