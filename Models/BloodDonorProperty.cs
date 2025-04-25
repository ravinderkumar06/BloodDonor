using System.ComponentModel.DataAnnotations;

namespace BloodDonor.Models
{
    public class BloodDonorProperty
    {

        [Key]
        public int DonorId { get; set; }

        [Required]
        [StringLength(100)]
        public string? FullName { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(8)]
        public string? Password { get; set; }



        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [StringLength(8)]
        public string? ConfirmPassword { get; set; }


        [Required]
        [DataType(DataType.PhoneNumber)]
        public long PhoneNumber { get; set; }

        [Required]
        [StringLength(3)]
        public string? BloodGroup { get; set; } // Changed from BloodType to BloodGroup

        [Required]
        [Range(18, 65, ErrorMessage = "Age must be between 18 and 65.")]
        public int Age { get; set; }

        [Required]
        public string? Gender { get; set; }

        [Required]
        public string? State { get; set; }

        [Required]
        public string? City { get; set; }

    }
}
