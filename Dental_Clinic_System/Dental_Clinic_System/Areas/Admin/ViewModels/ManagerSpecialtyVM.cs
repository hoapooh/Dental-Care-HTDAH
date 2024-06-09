using System.ComponentModel.DataAnnotations;

namespace Dental_Clinic_System.Areas.Admin.ViewModels
{
	public class ManagerSpecialtyVM
	{
		public int Id { get; set; }

		public string Name { get; set; } = null!;

		public string Image { get; set; } = null!;
		public string? Description { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}
