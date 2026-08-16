using System;
using System.Security.Cryptography;
using System.Text;

namespace SchoolResultManagementSystem.Helpers
{

    public static class PasswordHelper
    {
        public static string Hash(string plainTextPassword)
        {
            if (plainTextPassword == null) throw new ArgumentNullException(nameof(plainTextPassword));

            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(plainTextPassword));
                var builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static bool Verify(string plainTextPassword, string storedHash)
        {
            return string.Equals(Hash(plainTextPassword), storedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
