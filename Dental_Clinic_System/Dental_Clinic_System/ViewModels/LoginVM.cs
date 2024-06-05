using System.ComponentModel.DataAnnotations;

namespace Dental_Clinic_System.ViewModels
{
    public class LoginVM
    {
        [Display(Name = "Tên đăng nhập")]
        [Required(ErrorMessage = "* Vui lòng nhập tên đăng nhập")]
        [MaxLength(30, ErrorMessage = "Tối đa 30 ký tự")]
        public string Username { get; set; }
        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "* Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [MaxLength(30, ErrorMessage = "Tối đa 30 ký tự")]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
