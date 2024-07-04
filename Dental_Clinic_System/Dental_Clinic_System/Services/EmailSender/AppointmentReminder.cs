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
    using Dental_Clinic_System.Helper;
    using Microsoft.EntityFrameworkCore;
    using Dental_Clinic_System.Areas.Admin.ViewModels;
    using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

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

                        var now = Util.GetUtcPlus7Time();
                        var tomorrow = now.Date.AddDays(1);

                        var appointments = await context.Appointments.Include(a => a.PatientRecords).Include(a => a.Schedule).ThenInclude(s => s.TimeSlot).Where(a => a.Schedule.Date == DateOnly.FromDateTime(tomorrow)
                                        && (a.AppointmentStatus == "Đã Chấp Nhận" || a.AppointmentStatus == "Chờ Xác Nhận"))
                            .ToListAsync();

                        if (appointments.Count > 0)
                        {
                            foreach (var appointment in appointments)
                            {
                                var user = await context.Accounts.FirstOrDefaultAsync(a => a.ID == appointment.PatientRecords.AccountID);
                                if (user != null)
                                {
                                    var email = user.Email ?? "Rivinger7@gmail.com";
                                    var subject = "Nhắc nhở lịch hẹn khám";
                                    
                                    var message = $"Xin chào {user.FirstName},\n\nĐây là lời nhắc nhở về lịch hẹn khám của bạn vào ngày mai lúc {appointment.Schedule.TimeSlot.StartTime}.\n\nTrân trọng,\nDental Clinic System";
                                    await emailSender.SendEmailAsync(email, subject, message);

                                    // Wait for 20 seconds before sending the next email
                                    //await Task.Delay(TimeSpan.FromSeconds(120), stoppingToken);
                                }
                            }
                        }
                    }


                    _logger.LogInformation("Appointment reminders sent successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, "An error occurred while sending appointment reminders.");
                }

                // For testing
                //await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
                
            }
        }
    }
}
