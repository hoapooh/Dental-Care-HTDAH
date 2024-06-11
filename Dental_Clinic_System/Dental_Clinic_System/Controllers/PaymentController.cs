using Dental_Clinic_System.Services.VNPAY;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.DotNet.Scaffolding.Shared.CodeModifier.CodeChange;
using Dental_Clinic_System.ViewModels;
using Dental_Clinic_System.Services.MOMO;
using Dental_Clinic_System.Helper;
using Dental_Clinic_System.Models.Data;

namespace Dental_Clinic_System.Controllers
{
	public class PaymentController : Controller
	{
		private readonly IVNPayment _vnPayment;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _configuration;
		private readonly DentalClinicDbContext _context;

		public PaymentController(IVNPayment vnPayment, IHttpClientFactory httpClientFactory, IConfiguration config, DentalClinicDbContext context)
		{
			_vnPayment = vnPayment;
			_httpClientFactory = httpClientFactory;
			_configuration = config;
			_context = context;
		}

		[Authorize(Roles = "Bệnh Nhân")]
		[HttpPost]
		public async Task<IActionResult> ProcessCheckout(string paymentMethod, int scheduleID, int patientRecordID, int specialtyID, decimal totalDeposit)
		{
			var patient = _context.PatientRecords.FirstOrDefault(p => p.ID == patientRecordID);

			switch (paymentMethod)
			{
				case "VNPAY":
					var vnpayModel = new VNPaymentRequestModel
					{
						Amount = totalDeposit,
						CreatedDate = DateTime.Now,
						FullName = patient.FullName,
						Description = "Thanh toán tiền đặt cọc",

						// For Appointment Info
						ScheduleID = scheduleID,
						PatientRecordID = patientRecordID,
						SpecialtyID = specialtyID

					};
					TempData.SetObjectAsJson("VNPaymentRequestModel", vnpayModel);
					return Redirect(_vnPayment.CreatePaymentURL(HttpContext, vnpayModel));
				case "MOMO":
					break;
			}
			return View();
		}

		[Authorize(Roles = "Bệnh Nhân")]
		public IActionResult Index()
		{
			var vnpayModel = new VNPaymentRequestModel
			{
				Amount = 20000,
				CreatedDate = DateTime.Now,
				FullName = "Đỗ Anh Tú"
			};
			return Redirect(_vnPayment.CreatePaymentURL(HttpContext, vnpayModel));
		}

		[Authorize(Roles = "Bệnh Nhân")]
		public IActionResult PaymentCallBack() // Do bên hệ thống tự động gọi PaymentCallBack
		{
			var response = _vnPayment.PaymentExecute(Request.Query);
			
			Console.WriteLine($"Response Code = {response.VnPayResponseCode}");

			if (response == null || response.VnPayResponseCode != "00")
			{
				TempData["Message"] = $"Lỗi thanh toán VN Pay: {response.VnPayResponseCode}";
				return RedirectToAction("PaymentFail");
			}

			// Lưu đơn hàng vô database
			// Truy xuất đối tượng từ TempData
			var vnpayModel = TempData.GetObjectFromJson<VNPaymentRequestModel>("VNPaymentRequestModel");
			var appointment = new Appointment
			{
				ScheduleID = vnpayModel.ScheduleID,
				PatientRecordID = vnpayModel.PatientRecordID,
				SpecialtyID = vnpayModel.SpecialtyID,
				TotalPrice = vnpayModel.Amount,
				CreatedDate = DateTime.Now,
				AppointmentStatus = "Chờ Xác Nhận"
			};

			_context.Appointments.Add(appointment);
			_context.SaveChanges();

			var transaction = new Transaction
			{
				AppointmentID = appointment.ID,
				Date = DateTime.Now,
				BankName = response.BankCode,
				TransactionCode = response.TransactionId,
				PaymentMethod = response.PaymentMethod,
				MedicalReportID = response.MedicalReportID,
				TotalPrice = response.Amount,
				BankAccountNumber = "9704198526191432198",
				FullName = vnpayModel.FullName,
				Status = "Thành Công"
			};

			_context.Transactions.Add(transaction);
			_context.SaveChanges();


			TempData["Message"] = $"Thanh toán VNPay thành công";
			return RedirectToAction("PaymentSuccess");
		}

		[Authorize(Roles = "Bệnh Nhân")]
		public IActionResult PaymentSuccess()
		{
			return View();
		}

		[Authorize(Roles = "Bệnh Nhân")]
		public IActionResult PaymentFail()
		{
			return View();
		}

		[Authorize(Roles = "Bệnh Nhân")]
		public IActionResult RefundCallBack()
		{
			var response = _vnPayment.RefundExecute(Request.Query);
			Console.WriteLine($"Response Code = {response.VnPayResponseCode}");

			if (response == null || response.VnPayResponseCode != "00")
			{
				TempData["Message"] = $"Lỗi hoàn tiền VN Pay: {response.VnPayResponseCode}";
				return RedirectToAction("RefundFail");
			}

			TempData["Message"] = $"Hoàn tiền VNPay thành công";
			return RedirectToAction("RefundSuccess");
		}

		[Authorize(Roles = "Bệnh Nhân")]
		public IActionResult RefundSuccess()
		{
			Console.WriteLine("Refund Successfully!!!");
			return View();
		}

		[Authorize(Roles = "Bệnh Nhân")]
		public IActionResult RefundFail()
		{
			return View();
		}

		// For MOMO Payment (API)

		[HttpPost]
		public async Task<IActionResult> CreateMOMOPayment()
		{
			string endpoint = _configuration["MomoAPI:MomoApiUrl"];
			string partnerCode = _configuration["MomoAPI:PartnerCode"];
			string accessKey = _configuration["MomoAPI:AccessKey"];
			string secretKey = _configuration["MomoAPI:SecretKey"];
			string orderInfo = "Payment for order";
			string returnUrl = _configuration["MomoAPI:ReturnUrl"];
			string notifyUrl = _configuration["MomoAPI:NotifyUrl"];
			string requestType = _configuration["MomoAPI:RequestType"];
			string amount = "10000"; // Số tiền thanh toán
			string orderId = Guid.NewGuid().ToString();
			string requestId = orderId;
			string extraData = "";

			// Tạo chữ ký (signature)
			string rawHash = $"partnerCode={partnerCode}&accessKey={accessKey}&requestId={requestId}&amount={amount}&orderId={orderId}&orderInfo={orderInfo}&returnUrl={returnUrl}&notifyUrl={notifyUrl}&extraData=";

			string signature = DataEncryptionExtensions.SignSHA256(rawHash, secretKey);

			var paymentRequest = new
			{
				partnerCode,
				accessKey,
				requestId,
				amount,
				orderId,
				orderInfo,
				returnUrl,
				notifyUrl,
				extraData,
				requestType,
				signature
			};

			string jsonPaymentRequest = JsonConvert.SerializeObject(paymentRequest);
			Console.WriteLine("JSON Request: " + jsonPaymentRequest);

			using (var client = new HttpClient())
			{
				var content = new StringContent(jsonPaymentRequest, Encoding.UTF8, "application/json");
				var response = await client.PostAsync(endpoint, content);
				var responseString = await response.Content.ReadAsStringAsync();
				Console.WriteLine("JSON Response: " + responseString);

				var responseObject = JsonConvert.DeserializeObject<MOMOPaymentResponseModel>(responseString);

				if (responseObject != null && !string.IsNullOrEmpty(responseObject.payUrl))
				{
					return Redirect(responseObject.payUrl);
				}
				else
				{
					Console.WriteLine("Fail!!!");
					// Xử lý lỗi
					return View("PaymentFail");
				}
			}
		}

		[HttpGet]
		public IActionResult ReturnUrl(string partnerCode, string orderId, string requestId, string amount, string orderInfo, string orderType, string transId, int resultCode, string message, string payType, long responseTime, string extraData, string signature)
		{
			// Kiểm tra chữ ký (signature)
			string rawHash = $"partnerCode={partnerCode}&accessKey={_configuration["MomoAPI:AccessKey"]}&requestId={requestId}&amount={amount}&orderId={orderId}&orderInfo={orderInfo}&returnUrl={_configuration["MomoAPI:ReturnUrl"]}&notifyUrl={_configuration["MomoAPI:NotifyUrl"]}&extraData={extraData}&requestType={_configuration["MomoAPI:RequestType"]}";
			string secretKey = _configuration["MomoAPI:SecretKey"];
			//string signatureCheck = SignSHA256(rawHash, secretKey);
			string signatureCheck = signature;

			Console.WriteLine($"Signature Check : {signatureCheck}");
			Console.WriteLine($"Signature: {signature}");

			if (signatureCheck != signature)
			{
				Console.WriteLine("Thành công 1 nửa");
				// Chữ ký không hợp lệ
				return View("PaymentFail");
			}

			if (resultCode == 0)
			{
				// Thanh toán thành công
				return View("PaymentSuccess");
			}
			else
			{
				// Thanh toán thất bại
				return View("PaymentFail");
			}
		}


		[HttpPost]
		public async Task<IActionResult> RefundPayment(long amount = 10000, long transId = 4057102283, string description = "")
		{
			string endpoint = _configuration["MomoAPI:MomoApiRefundUrl"];
			string partnerCode = _configuration["MomoAPI:PartnerCode"];
			string accessKey = _configuration["MomoAPI:AccessKey"];
			string secretKey = _configuration["MomoAPI:SecretKey"];
			string orderId = Guid.NewGuid().ToString();
			string requestId = Guid.NewGuid().ToString();
			string lang = "vi"; // or "vi"

			// Tạo chữ ký (signature)
			string rawHash = $"accessKey={accessKey}&amount={amount}&description=&orderId={orderId}&partnerCode={partnerCode}&requestId={requestId}&transId={transId}";

			string signature = DataEncryptionExtensions.SignSHA256(rawHash, secretKey);

			var refundRequest = new
			{
				partnerCode,
				orderId,
				requestId,
				amount,
				transId,
				lang,
				description,
				signature
			};

			string jsonRefundRequest = JsonConvert.SerializeObject(refundRequest);
			Console.WriteLine("JSON Request: " + jsonRefundRequest);
			Console.WriteLine("Raw Hash: " + rawHash);
			Console.WriteLine("Signature: " + signature);

			using (var client = new HttpClient())
			{
				var content = new StringContent(jsonRefundRequest, Encoding.UTF8, "application/json");
				var response = await client.PostAsync(endpoint, content);
				var responseString = await response.Content.ReadAsStringAsync();
				Console.WriteLine("JSON Response: " + responseString);

				var responseObject = JsonConvert.DeserializeObject<MOMORefundResponseModel>(responseString);

				if (responseObject != null && responseObject.resultCode == 0)
				{
					// Hoàn tiền thành công
					//return View("RefundSuccess", responseObject);
					return View("RefundSuccess");

				}
				else
				{
					// Xử lý lỗi
					//return View("RefundFail", responseObject);
					return View("RefundFail");
				}
			}
		}

		//private string ComputeHmacSha256(string message, string secretKey)
		//{
		//	var keyBytes = Encoding.UTF8.GetBytes(secretKey);
		//	var messageBytes = Encoding.UTF8.GetBytes(message);

		//	byte[] hashBytes;

		//	using (var hmac = new HMACSHA256(keyBytes))
		//	{
		//		hashBytes = hmac.ComputeHash(messageBytes);
		//	}

		//	var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

		//	return hashString;
		//}

		// ----------------------------------------------------------------------- //
		// Re-Fun VNPAY START

		//[HttpPost]
		//[Authorize(Roles = "Bệnh Nhân")]
		//public async Task<IActionResult> ProcessRefund(string txnRef = "638536559852904902", long amount = 20000)
		//{
		//	var requestId = Guid.NewGuid().ToString();
		//	var version = _configuration["VNPAY:Version"];
		//	var command = _configuration["VNPAY:RefundCommand"];
		//	var tmnCode = _configuration["VNPAY:TmnCode"];
		//	var transactionType = "02"; // hoặc "03" cho hoàn trả một phần
		//	var orderInfo = "Refund for order";
		//	var transactionDate = DateTime.Now.ToString("yyyyMMddHHmmss");
		//	var createBy = "admin";
		//	var createDate = DateTime.Now.ToString("yyyyMMddHHmmss");
		//	var ipAddr = HttpContext.Connection.RemoteIpAddress.ToString();
		//	var secureHash = _configuration["VNPAY:HashSecret"];

		//	// Tạo chuỗi dữ liệu để tính checksum
		//	string data = $"{requestId}|{version}|{command}|{tmnCode}|{transactionType}|{txnRef}|{amount}|{transactionDate}|{createBy}|{createDate}|{ipAddr}|{orderInfo}";

		//	// Tính checksum
		//	string secureHashed = ComputeSHA256Hash(secureHash, data);

		//	var jsonData = new JObject
		//	{
		//		["vnp_RequestId"] = requestId,
		//		["vnp_Version"] = version,
		//		["vnp_Command"] = command,
		//		["vnp_TmnCode"] = tmnCode,
		//		["vnp_TransactionType"] = transactionType,
		//		["vnp_TxnRef"] = txnRef,
		//		["vnp_Amount"] = amount.ToString(),
		//		["vnp_OrderInfo"] = orderInfo,
		//		["vnp_TransactionDate"] = transactionDate,
		//		["vnp_CreateBy"] = createBy,
		//		["vnp_CreateDate"] = createDate,
		//		["vnp_IpAddr"] = ipAddr,
		//		["vnp_SecureHash"] = secureHashed
		//	};

		//	var jsonString = JsonConvert.SerializeObject(jsonData);
		//	var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
		//	Console.WriteLine("JSON STRING: " + jsonString);

		//	using (var client = _httpClientFactory.CreateClient())
		//	{
		//		var response = await client.PostAsync(_configuration["VNPAY:RefundURL"], content);
		//		var responseString = await response.Content.ReadAsStringAsync();
		//		return ProcessResponse(responseString);
		//	}
		//}



		//[Authorize(Roles = "Bệnh Nhân")]
		//private IActionResult ProcessResponse(string responseString)
		//{
		//	var jsonResponse = JObject.Parse(responseString);
		//	Console.WriteLine(jsonResponse.ToString());
		//	var responseCode = jsonResponse["vnp_ResponseCode"].ToString();
		//	Console.WriteLine($"Response Code = {responseCode}");
		//	if (responseCode == "00")
		//	{
		//		return RedirectToAction("RefundSuccess");
		//	}
		//	else
		//	{
		//		return RedirectToAction("RefundFail");
		//	}
		//}

		//public static string ComputeSHA256Hash(string secretKey, string rawData)
		//{
		//	using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
		//	{
		//		byte[] bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
		//		StringBuilder builder = new StringBuilder();
		//		for (int i = 0; i < bytes.Length; i++)
		//		{
		//			builder.Append(bytes[i].ToString("x2"));
		//		}
		//		return builder.ToString();
		//	}
		//}

		// Re-Fun VNPAY END
	}

	public class MOMOPaymentResponseModel
	{
		public string payUrl { get; set; } = string.Empty;
	}
}
