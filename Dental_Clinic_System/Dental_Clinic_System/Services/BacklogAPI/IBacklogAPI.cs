namespace Dental_Clinic_System.Services.BacklogAPI
{
    public interface IBacklogAPI
    {
        public Task GetSpaceInfoAsync();
        public Task SendErrorToWebhookAsync(string subject, string description, string categoryID);
    }
}
