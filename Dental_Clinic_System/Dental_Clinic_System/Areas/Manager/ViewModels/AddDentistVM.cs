using System.ComponentModel.DataAnnotations;

namespace Dental_Clinic_System.Areas.Manager.ViewModels
{
	public class AddDentistVM
	{
		public List<int> SpecialtyIDs { get; set; } = new List<int>();

		[Required(ErrorMessage = "Vui lòng nhập tên đăng nhập!")]
		[StringLength(30, ErrorMessage = "Tên đăng nhập không được vượt quá 30 ký tự.")]
		[MinLength(3, ErrorMessage = "Tên đăng nhập phải có ít nhất 3 ký tự.")]
		[RegularExpression("^[a-zA-Z][a-zA-Z0-9]*$", ErrorMessage = "Tên đăng nhập chỉ được chứa các ký tự chữ cái và số, và phải bắt đầu bằng chữ cái.")]
		public string Username { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập mật khẩu!")]
		[DataType(DataType.Password)]
		[StringLength(30, ErrorMessage = "Mật khẩu không được vượt quá 30 ký tự")]
		[MinLength(3, ErrorMessage = "Mật khẩu phải có ít nhất 3 ký tự")]
		[RegularExpression("^[a-zA-Z][a-zA-Z0-9]*$", ErrorMessage = "Mật khẩu chỉ được chứa các ký tự chữ cái và số, và phải bắt đầu bằng chữ cái.")]
		public string Password { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập tên!")]
		[RegularExpression(@"^[\p{L} ]*$", ErrorMessage = "Tên chỉ được chứa chữ cái và dấu cách!")]
		public string FirstName { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập họ (và tên đệm)!")]
		[RegularExpression(@"^[\p{L} ]*$", ErrorMessage = "Họ (và tên đệm) chỉ được chứa chữ cái và dấu cách!")]
		public string LastName { get; set; }

		public string Gender { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập email!")]
		[EmailAddress(ErrorMessage = "Email không hợp lệ.")]
		[StringLength(50, ErrorMessage = "Email không được quá 50 ký tự.")]
		[MinLength(3, ErrorMessage = "Email phải có ít nhất 3 ký tự.")]
		public string Email { get; set; } = null!;

		[Required(ErrorMessage = "Vui lòng nhập số điện thoại!")]
		[Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
		[RegularExpression(@"^\d{9,11}$", ErrorMessage = "Số điện thoại chỉ chứa các chữ số - độ dài từ 9 đến 11 ký tự .")]
		public string PhoneNumber { get; set; }

		public int DegreeID { get; set; }

        public string? Description { get; set; }


    }
}
