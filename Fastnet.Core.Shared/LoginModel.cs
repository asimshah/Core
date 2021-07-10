using System.ComponentModel.DataAnnotations;

namespace Fastnet.Core.Shared
{
    ///// <summary>
    ///// 
    ///// </summary>
    //public record UserAccountDTO(long Id, string Email);
    /// <summary>
    /// 
    /// </summary>
    public class LoginModel
    {
        /// <summary>
        /// 
        /// </summary>
        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
    }
}
