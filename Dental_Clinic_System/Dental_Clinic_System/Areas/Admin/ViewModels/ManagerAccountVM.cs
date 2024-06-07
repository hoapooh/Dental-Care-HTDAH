using System.ComponentModel.DataAnnotations;

namespace Dental_Clinic_System.ViewModels
{
	public class ManagerAccountVM
	{
		public int Id { get; set; }

		[Required(ErrorMessage = "Tên người dùng là bắt buộc.")]
		[MaxLength(30, ErrorMessage = "Tối đa 30 ký tự")]
		public string Username { get; set; } = null!;

		[Required(ErrorMessage = "Vui lòng nhập Email")]
		public string Email { get; set; } = null!;

		public string? PhoneNumber { get; set; }

		public string? Address { get; set; }

		//public string Gender { get; set; }

		public string Role { get; set; }

		public string Status { get; set; }

		public bool IsHidden { get; set; }
	}
}
