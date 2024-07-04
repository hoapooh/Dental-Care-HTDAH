namespace Dental_Clinic_System.ViewModels
{
    public class OrderVM
    {
        public int ID { get; set; }
        public string CompanyName { get; set; } = null!;
        public string CompanyPhonenumber { get; set; } = null!;
        public string CompanyEmail { get; set; } = null!;
        public string RepresentativeName { get; set; } = null!;
        public string ClinicName { get; set; } = null!;
        public string ClinicAddress { get; set; } = null!;
        public string? DomainName { get; set; }
        public string Content { get; set; } = null!;
        public string Image { get; set; } = null!;
        public string? ProvinceName { get; set; }
        public string? DistrictName { get; set; }
        public string? WardName { get; set; }
        public string PmWorkTime { get; set; } = null!;
        public string AmWorkTime { get; set; } = null!;
        public string? Status { get; set; }

    }
}
