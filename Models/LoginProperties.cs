using System.ComponentModel.DataAnnotations;

namespace BloodDonor.Models
{
    public class LoginProperties
    {
        [Range(0, int.MaxValue, ErrorMessage = "Id cannot be negative.")]
        public int ID { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string? Password { get; set; }


    }
}
