using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dental_Clinic_System.Helper;
using Dental_Clinic_System.Models.Data;
using Dental_Clinic_System.Services.MOMO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dental_Clinic_System.Areas.Dentist.Services
{
	public class AppointmentStatusUpdater : BackgroundService
	{
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly ILogger<AppointmentStatusUpdater> _logger;

		public AppointmentStatusUpdater(IServiceScopeFactory scopeFactory, ILogger<AppointmentStatusUpdater> logger)
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
						var momoPayment = scope.ServiceProvider.GetRequiredService<IMOMOPayment>();

						var now = Util.GetUtcPlus7Time();
						var appointments = context.Appointments
							.Include(a => a.Schedule)
								.ThenInclude(s => s.TimeSlot)
							.Include(a => a.Transactions)
							.Where(a => a.AppointmentStatus == "Chờ Xác Nhận") // Nếu không xác nhận thì sẽ thực hiện cái dưới
							.ToList();

						var periodicAppointment = context.PeriodicAppointments
												.Where(pa => pa.PeriodicAppointmentStatus == "Đã Chấp Nhận").ToList();
						
						foreach (var appointment in appointments)
						{
							//Điều kiện để dc hoàn tiền:  1. Nha sĩ ko xác nhận cho tới trước giờ bắt đầu khám
							//							  2. Thời gian hiện tại lớn hơn hoặc bằng 12h so với thời gian tạo hẹn
							if (appointment.Schedule.Date.ToDateTime(appointment.Schedule.TimeSlot.StartTime) < now || appointment.CreatedDate.Value.AddHours(12) <= now)
							{

								//Refund lại tiền nếu quá giờ mà chưa xác nhận
								var transactionCode = appointment.Transactions.FirstOrDefault()?.TransactionCode;
								var amount = appointment.Transactions.FirstOrDefault()?.TotalPrice;
								var bankName = appointment.Transactions.FirstOrDefault()?.BankName;
								var fullName = appointment.Transactions.FirstOrDefault()?.FullName;
								//Hàm hoàn tiền 
								var response = await momoPayment.RefundPayment((long)decimal.Parse(amount.ToString()), long.Parse(transactionCode), "");
								if (response != null)
								{
									var refundTransaction = new Transaction
									{
										AppointmentID = appointment.ID,
										Date = now,
										BankName = bankName,
										TransactionCode = response.transId.ToString(),
										PaymentMethod = "MOMO",
										TotalPrice = Decimal.Parse(response.amount.ToString()),
										BankAccountNumber = "9704198526191432198",
										FullName = fullName,
										Message = "Hoàn tiền thành công do phòng khám quá giờ xác nhận đơn khám",
										Status = "Thành Công"
									};
								}
								// Cập nhật trạng thái
								appointment.AppointmentStatus = "Đã Hủy";
							}
						}

						foreach (var periodic in periodicAppointment)
						{
							if (periodic.DesiredDate.ToDateTime(periodic.EndTime).AddHours(2) <= now) //Nếu như thời gian kết thúc khám định kỳ mà đã quá 2 tiếng thì cho hủy đơn, ko có hoàn tiền
							{
								periodic.PeriodicAppointmentStatus = "Đã Hủy";

							}
						}

						await context.SaveChangesAsync();
					}

					_logger.LogInformation("Appointment status change successfully.");

					
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Có lỗi xảy ra khi update lại status");
					ex.StackTrace.ToString();
				}

				await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Chạy lại sau mỗi 1 phút
			}
		}
	}
}
