using System.Security.Cryptography;
using System.Text;

namespace TravelAgency_Secure.Helpers
{
    public static class PasswordHelper
    {
        //
        //
        // RELEVANT PART !!
        //
        //
        public static string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // הפיכת הסיסמה למערך של בייטים
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                // הפיכת הבייטים למחרוזת הקסדצימלית
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        //
        // RELEVANT PART !!
        //
        public static bool VerifyPassword(string hashedPasswordFromDb, string inputPassword)
        {
            // מצפינים את הסיסמה שהוזנה ובודקים אם היא זהה למה שיש בבסיס הנתונים
            string hashOfInput = HashPassword(inputPassword);
            return hashOfInput == hashedPasswordFromDb;
        }
    }
}