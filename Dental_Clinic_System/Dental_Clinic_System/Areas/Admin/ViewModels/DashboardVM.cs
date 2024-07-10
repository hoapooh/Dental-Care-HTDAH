namespace Dental_Clinic_System.Areas.Admin.ViewModels
{
    public class DashboardVM
    {
        //public DateTime Date { get; set; }
        public int SelectedYear { get; set; }

        //public int SuccessfulAppointments { get; set; }

        //public int FailedAppointments { get; set; }

        public List<int> MonthlySuccessfulAppointments { get; set; }

        public List<int> MonthlyFailedAppointments { get; set; }

        public int AcceptedOrdersToday { get; set; }

        public int RejectedOrdersToday { get; set; }

        public List<int> MonthlyNewPost { get; set; }

		public List<string> ClinicNames { get; set; } = new List<string>();

		public List<double> ClinicRatings { get; set; } = new List<double>();
	}
}
