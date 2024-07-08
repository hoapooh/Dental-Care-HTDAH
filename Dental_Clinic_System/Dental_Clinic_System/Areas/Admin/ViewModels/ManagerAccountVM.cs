using Dental_Clinic_System.Models.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Dental_Clinic_System.Areas.Admin.ViewModels
{
    public class ManagerAccountVM
    {
        public int Id { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
        [StringLength(30, ErrorMessage = "Tên đăng nhập không được vượt quá 30 ký tự.")]
        [MinLength(3, ErrorMessage = "Tên đăng nhập phải có ít nhất 3 ký tự.")]
        [RegularExpression("^[a-zA-Z][a-zA-Z0-9]*$", ErrorMessage = "Tên đăng nhập chỉ được chứa các ký tự chữ cái và số, và phải bắt đầu bằng chữ cái.")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [StringLength(50, ErrorMessage = "Email không được quá 50 ký tự.")]
        [MinLength(3, ErrorMessage = "Email phải có ít nhất 3 ký tự.")]
        public string? Email { get; set; }

        public string? Gender { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [RegularExpression(@"^\d{9,11}$", ErrorMessage = "Số điện thoại phải có độ dài từ 9 đến 11 số.")]
        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public string Role { get; set; }

        public int? ProvinceId { get; set; }

        public int? DistrictId { get; set; }

        public int? WardId { get; set; }

		//public string Description { get; set; }

		public string? ClinicName { get; set; }

        public List<string> Specialties { get; set; }
    }
}



