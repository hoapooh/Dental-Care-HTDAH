namespace Dental_Clinic_System.Services
{
	public interface IEmailSenderCustom
	{
		Task SendEmailAsync(string email, string subject, string message);
		Task SendEmailForUpdatingAsync(string email, string username, string subject, string message);
        Task SendEmailUpdatedAsync(string oldEmail, string newEmail, string subject, string message);
		Task SendInvoiceEmailAsync(string email, string subject, string message);

    }
}
