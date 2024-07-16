namespace Dental_Clinic_System.Areas.Admin.ViewModels
{
    public class DashboardVM
    {
        //public DateTime Date { get; set; }
        public int SelectedYear { get; set; }

        public int SelectedMonth { get; set; }

        //public int SuccessfulAppointments { get; set; }

        //public int FailedAppointments { get; set; }

        public int SuccessfulAppointmentsCurrentYear { get; set; }

        public int SuccessfulAppointmentsPreviousYear { get; set; }

        public int FailedAppointmentsCurrentYear { get; set; }

        public int FailedAppointmentsPreviousYear { get; set; }

        //public int AcceptedOrdersToday { get; set; }

        //public int RejectedOrdersToday { get; set; }

        public List<int> AcceptedOrdersMonthly { get; set; }

        public List<int> RejectedOrdersMonthly { get; set; }

        public List<int> MonthlyNewPost { get; set; }

		public List<string> ClinicNames { get; set; } = new List<string>();

		public List<double> ClinicRatings { get; set; } = new List<double>();
	}
}
