using System.ComponentModel.DataAnnotations;

namespace Dental_Clinic_System.Areas.Manager.ViewModels
{
	public class EditDentistVM
	{
		public List<int> SpecialtyIDs { get; set; } = new List<int>();
		public int DentistId { get; set; }
        public int AccountId { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập tên!")]
		[RegularExpression(@"^[\p{L} ]*$", ErrorMessage = "Tên chỉ được chứa chữ cái và dấu cách!")]
		public string FirstName { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập họ (và tên đệm)!")]
		[RegularExpression(@"^[\p{L} ]*$", ErrorMessage = "Họ (và tên đệm) chỉ được chứa chữ cái và dấu cách!")]
		public string LastName { get; set; }
        public string Gender { get; set; }
        public int? Province { get; set; } = null!;

        public int? Ward { get; set; } = null!;

        public int? District { get; set; } = null!;

        public string? Address { get; set; } = null!;

        public DateOnly? DateOfBirth { get; set; }

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

        public string Status { get; set; }

		public string? Image {  get; set; }

		public string? NewPassWord { get; set; }
    }
}
