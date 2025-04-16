using System.ComponentModel.DataAnnotations;

namespace SecureBankingSystem.ViewModels
{
    public class TwoFactorViewModel
    {
        [Required]
        [Display(Name = "Verification Code")]
        public string VerificationCode { get; set; }

        public string Email { get; set; }

        public bool RememberMe { get; set; }
    }
}