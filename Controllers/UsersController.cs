using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using TravelAgency_Secure.Helpers;
using TravelAgency_Secure.Models;


namespace TravelAgency_Secure.Controllers
{
    public class UsersController : Controller
    {
        private readonly string _connStr;
        private readonly TravelAgency_Secure.Services.EmailService _emailService;

        public UsersController(IConfiguration config, TravelAgency_Secure.Services.EmailService emailService)
        {
            _connStr = config.GetConnectionString("DefaultConnection");
            _emailService = emailService;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Email and password are required";
                return View();
            }

            var emailValidator = new EmailAddressAttribute();
            if (!emailValidator.IsValid(email))
            {
                ViewBag.Error = "Invalid email format";
                return View();
            }


            using SqlConnection con = new SqlConnection(_connStr);
            string sql = "SELECT * FROM Users WHERE Email=@email AND IsActive=1";

            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@email", email);

            con.Open();
            var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                //
                // RELEVANT PART !!
                //
                string passwordHashFromDb = reader["PasswordHash"].ToString();

                bool isValidPassword =
                    PasswordHelper.VerifyPassword(passwordHashFromDb, password);

                if (!isValidPassword)
                {
                    ViewBag.Error = "Invalid login";
                    return View();
                }


                string role = reader["Role"].ToString();

                HttpContext.Session.SetString("UserName", reader["FullName"].ToString());
                HttpContext.Session.SetString("Role", role);
                HttpContext.Session.SetInt32("UserId", (int)reader["UserId"]);
                HttpContext.Session.SetString("UserEmail", reader["Email"].ToString());

                return RedirectToAction("Trips", "Trips");

            }

            ViewBag.Error = "Invalid login";
            return View();
        }

        //
        // RELEVANT PART !!
        //
        // GET: Users/ForgotPassword
       
        public IActionResult ForgotPassword()
        {
            ViewBag.Step = 1;  // הצגה של דף הזנת מייל
            return View();
        }

        //
        // RELEVANT PART !!
        //
        // שלב 1: שליחת קוד למייל
        [HttpPost]
        public IActionResult SendResetCode(string email)
        {
            using SqlConnection con = new SqlConnection(_connStr);
            con.Open();
            SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Email=@email", con);
            cmd.Parameters.AddWithValue("@email", email);
            
            if ((int)cmd.ExecuteScalar() == 0) {
                ViewBag.Error = "Email not found";
                ViewBag.Step = 1;
                return View("ForgotPassword");
            }

            string code = new Random().Next(100000, 999999).ToString();
            HttpContext.Session.SetString("ResetCode", code);
            HttpContext.Session.SetString("ResetEmail", email);

            _emailService.SendPasswordResetEmail(email, code);

            ViewBag.Step = 2; // עוברים לשלב הזנת הקוד
            return View("ForgotPassword");
        }

        // שלב 2: דף אימות הקוד
        public IActionResult VerifyCode() => View();
        //
        // RELEVANT PART !!
        //
        [HttpPost]
        public IActionResult VerifyCode(string inputCode)
        {
            if (inputCode == HttpContext.Session.GetString("ResetCode")) {
                ViewBag.Step = 3; // הקוד נכון, עוברים להזנת סיסמה
            } else {
                ViewBag.Error = "Invalid code";
                ViewBag.Step = 2;
            }
            return View("ForgotPassword");
        }
        // שלב 3: עדכון הסיסמה הסופי ב-SHA-256
        public IActionResult ResetPassword() => View();

        //
        // RELEVANT PART !!
        //
        [HttpPost]
        public IActionResult ResetPassword(string newPassword)
        {
            string email = HttpContext.Session.GetString("ResetEmail");
            string newHash = PasswordHelper.HashPassword(newPassword);

            using SqlConnection con = new SqlConnection(_connStr);
            con.Open();
            SqlCommand cmd = new SqlCommand("UPDATE Users SET PasswordHash=@hash WHERE Email=@email", con);
            cmd.Parameters.AddWithValue("@hash", newHash);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.ExecuteNonQuery();

            TempData["Success"] = "Password updated!";
            return RedirectToAction("Login");
        }
                
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ================= REGISTER =================

        // GET: Users/Register
        public IActionResult Register()
        {
            return View();
        }

        //
        // RELEVANT PART !!
        //
        // POST: Users/Register
        [HttpPost]
        public IActionResult Register(string fullName, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "All fields are required";
                return View();
            }

            var emailValidator = new EmailAddressAttribute();
            if (!emailValidator.IsValid(email))
            {
                ViewBag.Error = "Invalid email format";
                return View();
            }

            // בדיקת חוזק סיסמה (לפי הדרישות שלך)
            if (password.Length < 6 || !password.Any(char.IsUpper) || !password.Any(char.IsDigit))
            {
                ViewBag.Error = "Password must be at least 6 characters, include an uppercase letter and a number";
                return View();
            }

            using SqlConnection con = new SqlConnection(_connStr);
            con.Open();

            // בדיקה אם המייל קיים
            SqlCommand checkCmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Email=@email", con);
            checkCmd.Parameters.AddWithValue("@email", email);
            if ((int)checkCmd.ExecuteScalar() > 0)
            {
                ViewBag.Error = "Email already exists";
                return View();
            }

            // בדיקה אם אין משתמשים - המשתמש הראשון הוא מנהל
            SqlCommand countCmd = new SqlCommand("SELECT COUNT(*) FROM Users", con);
            int userCount = (int)countCmd.ExecuteScalar();
            string assignedRole = (userCount == 0) ? "Admin" : "User"; 

            // הצפנה ב-SHA-256
            string hash = PasswordHelper.HashPassword(password);

            // הכנסה ל-DB כולל ה-Role הדינמי
            SqlCommand insertCmd = new SqlCommand(
                @"INSERT INTO Users (FullName, Email, PasswordHash, Role, IsActive)
                VALUES (@fullName, @email, @hash, @role, 1)", con);

            insertCmd.Parameters.AddWithValue("@fullName", fullName);
            insertCmd.Parameters.AddWithValue("@email", email);
            insertCmd.Parameters.AddWithValue("@hash", hash);
            insertCmd.Parameters.AddWithValue("@role", assignedRole);

            insertCmd.ExecuteNonQuery();

            return RedirectToAction("Login");
        }




        // to fix the hash 1 timer!!!!!
        /*
        public IActionResult FixPasswords()
        {
            using SqlConnection con = new SqlConnection(_connStr);
            string sql = "SELECT UserId, PasswordHash FROM Users";

            SqlCommand cmd = new SqlCommand(sql, con);
            con.Open();

            var reader = cmd.ExecuteReader();
            List<(int id, string pass)> users = new();

            while (reader.Read())
            {
                users.Add((
                    (int)reader["UserId"],
                    reader["PasswordHash"].ToString()
                ));
            }

            reader.Close();

            foreach (var u in users)
            {
                string newHash = PasswordHelper.HashPassword(u.pass);

                SqlCommand updateCmd = new SqlCommand(
                    "UPDATE Users SET PasswordHash=@hash WHERE UserId=@id", con);

                updateCmd.Parameters.AddWithValue("@hash", newHash);
                updateCmd.Parameters.AddWithValue("@id", u.id);

                updateCmd.ExecuteNonQuery();
            }

            return Content("Passwords fixed");
        }
        */
    }
}
