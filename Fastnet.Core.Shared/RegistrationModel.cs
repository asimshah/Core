using System.ComponentModel.DataAnnotations;

namespace Fastnet.Core.Shared
{
    //public record RegistrationResult(bool IsSuccessfulRegistration);
    /// <summary>
    /// 
    /// </summary>
    public class RegistrationModel
    {
        /// <summary>
        /// 
        /// </summary>
        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        //public bool IsStreetRep { get; set; }
    }
}
