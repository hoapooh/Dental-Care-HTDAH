using System.ComponentModel.DataAnnotations;

namespace Dental_Clinic_System.ViewModels
{
	public class RegisterVM
	{
		[Display(Name = "Tên đăng nhập")]
		[Required(ErrorMessage = "* Vui lòng nhập tên đăng nhập")]
		[StringLength(30, ErrorMessage = "Tên đăng nhập không được vượt quá 30 ký tự")]
        [RegularExpression("^[a-zA-Z][a-zA-Z0-9]*$", ErrorMessage = "Tên đăng nhập chỉ được chứa các ký tự chữ cái và số, và phải bắt đầu bằng chữ cái.")]
        public string Username { get; set; }

		[Display(Name = "Mật khẩu")]
		[Required(ErrorMessage = "* Vui lòng nhập mật khẩu")]
		[DataType(DataType.Password)]
		[StringLength(30, ErrorMessage = "Mật khẩu không được vượt quá 30 ký tự")]
        [RegularExpression("^[a-zA-Z][a-zA-Z0-9]*$", ErrorMessage = "Mật khẩu chỉ được chứa các ký tự chữ cái và số, và phải bắt đầu bằng chữ cái.")]
        public string Password { get; set; }

		[Display(Name = "Email")]
		[Required(ErrorMessage = "* Vui lòng nhập email")]
		[EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
		[StringLength(50, ErrorMessage = "Email không được vượt quá 50 ký tự")]
		public string Email { get; set; }

		[Display(Name = "Số điện thoại")]
		[Required(ErrorMessage = "* Vui lòng nhập số điện thoại")]
		[Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
		[StringLength(11, ErrorMessage = "Số điện thoại không được vượt quá 11 ký tự")]
		public string PhoneNumber { get; set; }
    }
}
