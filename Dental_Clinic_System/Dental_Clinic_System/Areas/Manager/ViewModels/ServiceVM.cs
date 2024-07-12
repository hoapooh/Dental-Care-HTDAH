using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Dental_Clinic_System.Areas.Manager.ViewModels
{
    public class ServiceVM
    {
        public int ID { get; set; }

        public int ClinicID { get; set; }

        public int SpecialtyID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên dịch vụ!")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập đơn giá!")]
        public string Price { get; set; } = null!;
        public string? Description { get; set; }
    }
}
