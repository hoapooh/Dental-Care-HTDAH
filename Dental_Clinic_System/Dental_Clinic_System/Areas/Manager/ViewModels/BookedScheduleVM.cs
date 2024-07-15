namespace Dental_Clinic_System.Areas.Manager.ViewModels
{
    public class BookedScheduleVM
    {
        public string DentistName { get; set; }
        public DateOnly Date { get; set; }
        public string Start_EndTime { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string? Description { get; set; }
    }
}
