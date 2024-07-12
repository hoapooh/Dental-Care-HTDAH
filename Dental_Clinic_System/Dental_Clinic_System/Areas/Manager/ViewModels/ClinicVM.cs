using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Dental_Clinic_System.Areas.Manager.ViewModels
{
    public class ClinicVM
    {
        public int ID { get; set; }

        public int ManagerID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên phòng khám!")]
        public string Name { get; set; } = null!;

        public int? Province { get; set; } = null!;

        public int? Ward { get; set; } = null!;

        public int? District { get; set; } = null!;

        public string? Address { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại!")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [RegularExpression(@"^\d{9,11}$", ErrorMessage = "Số điện thoại chỉ chứa các chữ số - độ dài từ 9 đến 11 ký tự .")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email!")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [StringLength(50, ErrorMessage = "Email không được quá 50 ký tự.")]
        [MinLength(3, ErrorMessage = "Email phải có ít nhất 3 ký tự.")]
        public string Email { get; set; }

        public string? Description { get; set; }

        public string? Image { get; set; } = null!;

        public string ClinicStatus { get; set; } = null!;

        public string? MapLinker { get; set; }

        //public int AmWorkTimeID { get; set; }

        //public int PmWorkTimeID { get; set; }

        public TimeOnly AmStartTime { get; set; }

        public TimeOnly AmEndTime { get; set; }

        public TimeOnly PmStartTime { get; set; }

        public TimeOnly PmEndTime { get; set; }

    }
}
