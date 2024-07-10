using System.ComponentModel.DataAnnotations;

namespace Dental_Clinic_System.Areas.Manager.ViewModels
{
    public class ManagerVM
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "* Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(11, ErrorMessage = "Số điện thoại không được vượt quá 11 ký tự")]
        [RegularExpression(@"^\d{9,11}$", ErrorMessage = "Số điện thoại phải có độ dài từ 9 đến 11 ký tự và chỉ chứa các chữ số.")]
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "* Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
        [MinLength(3, ErrorMessage = "Email phải có ít nhất 3 ký tự")]
        [StringLength(50, ErrorMessage = "Email không được vượt quá 50 ký tự")]
        public string Email { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public int? Province { get; set; }
        public int? District { get; set; }
        public int? Ward { get; set; }
        public string? Address { get; set; }
        public string? Image { get; set; }
    }
}
