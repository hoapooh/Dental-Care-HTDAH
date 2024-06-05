using System.ComponentModel.DataAnnotations;

namespace Dental_Clinic_System.ViewModels
{
	public class ChangePasswordVM
	{
		[Required(ErrorMessage = "* Vui lòng nhập mật khẩu")]
		[DataType(DataType.Password)]
		[MaxLength(30, ErrorMessage = "Tối đa 30 ký tự")]
		public string Password { get; set; }
		[Required(ErrorMessage = "* Vui lòng nhập mật khẩu mới")]
		[DataType(DataType.Password)]
		[MaxLength(30, ErrorMessage = "Tối đa 30 ký tự")]
		public string NewPassword { get; set; }
		[Required(ErrorMessage = "* Vui lòng nhập mật khẩu xác nhận")]
		[DataType(DataType.Password)]
		[MaxLength(30, ErrorMessage = "Tối đa 30 ký tự")]
		public string ConfirmPassword { get; set;}
	}
}
