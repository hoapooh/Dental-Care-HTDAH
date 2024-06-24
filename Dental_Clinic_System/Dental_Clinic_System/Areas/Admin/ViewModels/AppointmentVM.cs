namespace Dental_Clinic_System.Areas.Admin.ViewModels
{
    public class AppointmentVM
    {
        //public DateTime Date { get; set; }
        public int SuccessfulAppointments { get; set; }
        public int FailedAppointments { get; set; }
        public List<int> MonthlySuccessfulAppointments { get; set; }
        public List<int> MonthlyFailedAppointments { get; set; }
    }
}
