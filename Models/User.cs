using System.ComponentModel.DataAnnotations;

namespace TravelAgency_Secure.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; }

        [Required]
        public string Role { get; set; }   // "Admin" / "User"

        public bool IsActive { get; set; }

        // ===== Credit Card Details (Classwork 2) =====

        [Required]
        [RegularExpression(@"^[A-Za-zא-ת]+$", ErrorMessage = "First name must contain only letters")]
        public string FirstName { get; set; }

        [Required]
        [RegularExpression(@"^[A-Za-zא-ת]+$", ErrorMessage = "Last name must contain only letters")]
        public string LastName { get; set; }

        [Required]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "ID must be 9 digits")]
        public string IDNumber { get; set; }

        [Required]
        [RegularExpression(@"^\d{4} \d{4} \d{4} \d{4}$", ErrorMessage = "Credit card must be in format 1234 5678 9012 3456")]
        public string CreditCardNumber { get; set; }

        [Required]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "Date must be MM/YY")]
        public string ValidDate { get; set; }

        [Required]
        [RegularExpression(@"^\d{3}$", ErrorMessage = "CVC must be 3 digits")]
        public string CVC { get; set; }

        public List<Booking> Bookings { get; set; } = new List<Booking>();


    }
}
