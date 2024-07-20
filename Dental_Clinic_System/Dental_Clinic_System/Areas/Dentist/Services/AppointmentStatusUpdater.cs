using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dental_Clinic_System.Helper;
using Dental_Clinic_System.Models.Data;
using Dental_Clinic_System.Services.EmailSender;
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
						var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSenderCustom>();

						var now = Util.GetUtcPlus7Time();
						var appointments = context.Appointments
							.Include(a => a.Schedule)
								.ThenInclude(s => s.TimeSlot)
							.Include(a => a.Transactions)
							.Include(a => a.PatientRecords)
							.ToList();

						var periodicAppointment = context.PeriodicAppointments
												.Where(pa => pa.PeriodicAppointmentStatus == "Đã Chấp Nhận").ToList();

						var pendingAppointments = appointments.Where(a => a.AppointmentStatus == "Chờ Xác Nhận").ToList();

						foreach (var appointment in pendingAppointments)
						{
							//Điều kiện để dc hoàn tiền:  1. Nha sĩ ko xác nhận cho tới trước giờ bắt đầu khám
							//							  2. Thời gian hiện tại lớn hơn hoặc bằng 12h so với thời gian tạo hẹn
							if (appointment.Schedule.Date.ToDateTime(appointment.Schedule.TimeSlot.StartTime) < now || appointment.CreatedDate.Value.AddHours(24) <= now)
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
									//Nếu như đã hoàn tiền / hoàn tiền thất bại / số dư hoàn tiền không hợp lệ thì bỏ qua
									if (response.message == "Yêu cầu bị từ chối vì số tiền giao dịch không hợp lệ." || response.resultCode != 0)
									{
										continue;
									}
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

									//Thêm transaction vào DB
									context.Transactions.Add(refundTransaction);

									// Cập nhật trạng thái
									appointment.AppointmentStatus = "Đã Hủy";
									// Gửi lại mail xác nhận đã hủy cho khách hàng
									var user = await context.Accounts.FirstOrDefaultAsync(a => a.ID == appointment.PatientRecords.AccountID);
									if (user != null)
									{
										var email = user.Email ?? "Rivinger7@gmail.com";
										var subject = "Nhắc nhở hủy khám";

										var message = $@"<p>Xin chào <strong>{user.FirstName}</strong></p>," +
													  $"<p>Đây là thông báo về việc hủy lịch khám vào ngày <strong>{appointment.Schedule.Date.ToString("dd/MM/yyyy")}</strong> lúc <strong>{appointment.Schedule.TimeSlot.StartTime.ToString("HH:mm")}</strong><br>." +
													  $"<p><strong>Lý do:</strong> Quá giờ nha sĩ xác nhận đơn đặt khám của quý khách. Số tiền đặt cọc sẽ được hoàn trả lại vào tài khoản của quý khách, vui lòng kiểm tra.</p>" +
													  $"<p>Trân trọng,</p>" +
													  $"<p>Dental Care.</p><br>" +
													  $"<img src='https://firebasestorage.googleapis.com/v0/b/dental-care-3388d.appspot.com/o/Dental%20Care%20Logo%2FDentalCare.png?alt=media&token=8854a154-1dde-4aa3-b573-f3c0aca83776' alt='logo DentalCare'>";
										await emailSender.SendEmailAsync(email, subject, message);
									}
									appointment.Description = "Đã Hủy đơn khám do quá giờ nha sĩ xác nhận đơn khám + Đã hoàn tiền.";
								}
							}
						}

						var acceptAppointment = appointments.Where(a => a.AppointmentStatus == "Đã Chấp Nhận").ToList();
						foreach (var appointment in acceptAppointment)
						{
							if (appointment.Schedule.Date.ToDateTime(appointment.Schedule.TimeSlot.EndTime).AddMinutes(30) < now) // Nếu sau 30 phút kể từ thời gian kết thúc khám mà chưa thay đổi trạng thái thì hủy đơn, không hoàn tiền
							{
								appointment.AppointmentStatus = "Đã Hủy";
								appointment.Description = "Đã Hủy đơn khám do quá giờ nha sĩ xác nhận đơn khám sau khi khám + Không hoàn tiền.";
							}
						}

						foreach (var periodic in periodicAppointment)
						{
							if (periodic.DesiredDate.ToDateTime(periodic.EndTime).AddMinutes(30) < now) //Nếu như thời gian kết thúc khám định kỳ mà đã quá 30 phút thì cho hủy đơn, ko có hoàn tiền (vì không có cọc gì ở periodic appointment)
							{
								periodic.PeriodicAppointmentStatus = "Đã Hủy";
								periodic.Description = "Đã Hủy đơn khám định kỳ do quá giờ nha sĩ xác nhận đơn khám sau khi khám + Không hoàn tiền.";

							}
						}

						await context.SaveChangesAsync();
					}

					_logger.LogInformation("///////////-Appointment status change successfully.-///////////");


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
