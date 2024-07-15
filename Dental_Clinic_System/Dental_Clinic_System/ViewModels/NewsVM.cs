namespace Dental_Clinic_System.ViewModels
{
    public class NewsVM
    {
        public int ID { get; set; }
        public int AccountID { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? ThumbNail { get; set; }
        public string? Status { get; set; }
    }
}
