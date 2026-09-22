namespace MiniB2B.Business.Security;

public static class PasswordHasher
{
    private const int WorkFactor = 11;

    public static string Hash(string password)
        => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public static bool Verify(string password, string passwordHash)
        => BCrypt.Net.BCrypt.Verify(password, passwordHash);
}
