using System.Security.Cryptography;

namespace ClinicAssistant.Infrastructure.Identity;

public static class PasswordHasher
{
    private const int Iterations = 210_000;
    private const int KeyLength = 32;

    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA512, KeyLength);
        return $"v1.{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string password, string storedHash)
    {
        var values = storedHash.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (values.Length != 4 || values[0] != "v1" || !int.TryParse(values[1], out var iterations)) return false;

        try
        {
            var salt = Convert.FromBase64String(values[2]);
            var expectedHash = Convert.FromBase64String(values[3]);
            var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA512, expectedHash.Length);
            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
