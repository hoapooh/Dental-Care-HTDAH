namespace Dental_Clinic_System.Services.MOMO
{
    public class MOMOPaymentRequestModel
    {
        public long Amount {  get; set; }
        public string OrderInformation { get; set; }
        public string OrderID { get; set; }

        // For Appointment Info
        public int ScheduleID { get; internal set; }
        public int PatientRecordID { get; internal set; }
        public int SpecialtyID { get; internal set; }
        public string FullName { get; internal set; }
    }
}
