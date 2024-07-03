namespace Dental_Clinic_System.Services.EmailSender
{
    using Dental_Clinic_System.Models.Data;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class AppointmentReminder : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AppointmentReminder> _logger;

        public AppointmentReminder(IServiceScopeFactory scopeFactory, ILogger<AppointmentReminder> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<DentalClinicDbContext>();
                        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSenderCustom>();

                        // Get the current UTC time
                        var now = DateTime.UtcNow;
                        var reminderTime = now.AddDays(1);

                        var appointments = context.Appointments
                            .Where(a => a.Schedule.Date == DateOnly.FromDateTime(reminderTime.Date)
                                && a.Schedule.TimeSlot.StartTime.Hour == reminderTime.Hour
                                && a.Schedule.TimeSlot.StartTime.Minute == reminderTime.Minute
                                && a.AppointmentStatus == "Đã Chấp Nhận")
                            .ToList();

                        foreach (var appointment in appointments)
                        {
                            var user = context.Accounts.Find(appointment.PatientRecordID);
                            if (user != null)
                            {
                                var email = user.Email;
                                var subject = "Nhắc nhở lịch hẹn khám";
                                var message = $"Xin chào {user.FirstName},\n\nĐây là lời nhắc nhở về lịch hẹn khám của bạn vào ngày mai lúc {appointment.Schedule.TimeSlot.StartTime}.\n\nTrân trọng,\nDental Clinic System";
                                await emailSender.SendEmailConfirmationAsync(email, subject, message);
                            }
                        }
                    }

                    _logger.LogInformation("Appointment reminders sent successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while sending appointment reminders.");
                }

                // Wait for 24 hours before checking again
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}
