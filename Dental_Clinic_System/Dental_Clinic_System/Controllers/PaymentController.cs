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
using Azure;
using Microsoft.CodeAnalysis;

namespace Dental_Clinic_System.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IVNPayment _vnPayment;
        private readonly IMOMOPayment _momoPayment;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly DentalClinicDbContext _context;

        public PaymentController(IVNPayment vnPayment, IMOMOPayment momoPayment, IHttpClientFactory httpClientFactory, IConfiguration config, DentalClinicDbContext context)
        {
            _vnPayment = vnPayment;
            _httpClientFactory = httpClientFactory;
            _configuration = config;
            _context = context;
            _momoPayment = momoPayment;

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
                    var momoModel = new MOMOPaymentRequestModel
                    {
                        OrderID = Guid.NewGuid().ToString(),
                        OrderInformation = "Thanh toán tiền đặt cọc",
                        FullName = patient.FullName,
                        Amount = (long)totalDeposit,

                        // For Appointment Info
                        ScheduleID = scheduleID,
                        PatientRecordID = patientRecordID,
                        SpecialtyID = specialtyID
                    };
                    TempData.SetObjectAsJson("MOMOPaymentRequestModel", momoModel);
                    return Redirect(_momoPayment.CreatePaymentURL(momoModel).Result);
            }
            return View();
        }

        [Authorize(Roles = "Bệnh Nhân")]
        public IActionResult Index()
        {
            var momoModel = new MOMOPaymentRequestModel
            {
                OrderID = Guid.NewGuid().ToString(),
                OrderInformation = "Thanh toán tiền đặt cọc",
                Amount = 10000,

                // For Appointment Info
                ScheduleID = 0,
                PatientRecordID = 0,
                SpecialtyID = 0
            };
            var vnpayModel = new VNPaymentRequestModel
            {
                Amount = 50000,
                CreatedDate = DateTime.Now,
                FullName = "aaaa",
                Description = "Thanh toán tiền đặt cọc",

                // For Appointment Info
                ScheduleID = 0,
                PatientRecordID = 0,
                SpecialtyID = 0

            };
            //return Redirect(_momoPayment.CreatePaymentURL(momoModel).Result);
            return Redirect(_vnPayment.CreatePaymentURL(HttpContext, vnpayModel));
        }

        [Authorize(Roles = "Bệnh Nhân")]
        public IActionResult PaymentResult()
        {
            return View();
        }

        [Authorize(Roles = "Bệnh Nhân")]
        public IActionResult PaymentInvoice()
        {
            return View();
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


        // For VN PaymentReturnURL

        [Authorize(Roles = "Bệnh Nhân")]
        public IActionResult PaymentCallBack() // Do bên hệ thống tự động gọi PaymentCallBack
        {
            var response = _vnPayment.PaymentExecute(Request.Query);

            Console.WriteLine($"Response Code = {response.VnPayResponseCode}");

            if (response == null || response.VnPayResponseCode != "00")
            {
                ViewBag.VnPayResponseCode = response.VnPayResponseCode;
                ViewBag.Message = "Lỗi thanh toán VN Pay";
                return View("PaymentResult");
            }

            // Đang test nên tạm thời chưa lưu vào database

            // Lưu đơn hàng vô database
            // Truy xuất đối tượng từ TempData
            //var vnpayModel = TempData.GetObjectFromJson<VNPaymentRequestModel>("VNPaymentRequestModel");
            //var appointment = new Appointment
            //{
            //    ScheduleID = vnpayModel.ScheduleID,
            //    PatientRecordID = vnpayModel.PatientRecordID,
            //    SpecialtyID = vnpayModel.SpecialtyID,
            //    TotalPrice = vnpayModel.Amount,
            //    CreatedDate = DateTime.Now,
            //    AppointmentStatus = "Chờ Xác Nhận"
            //};

            //_context.Appointments.Add(appointment);
            //_context.SaveChanges();

            //var transaction = new Transaction
            //{
            //    AppointmentID = appointment.ID,
            //    Date = DateTime.Now,
            //    BankName = response.BankCode,
            //    TransactionCode = response.TransactionId,
            //    PaymentMethod = response.PaymentMethod,
            //    TotalPrice = response.Amount,
            //    BankAccountNumber = "9704198526191432198",
            //    FullName = vnpayModel.FullName,
            //    Message = response.OrderDescription,
            //    Status = "Thành Công"
            //};

            //_context.Transactions.Add(transaction);
            //_context.SaveChanges();


            string responseCode = response.VnPayResponseCode;
            ViewBag.VnPayResponseCode = responseCode;
            ViewBag.Message = "Thanh toán VNPay thành công";
            return View("PaymentResult");
        }


        // For MOMO PaymentReturnURL (API)

        [HttpGet]
        public async Task<IActionResult> ReturnUrl(string partnerCode, string orderId, string requestId, string amount, string orderInfo, string orderType, string transId, int resultCode, string message, string payType, long responseTime, string extraData, string signature)
        {
            // Kiểm tra chữ ký (signature)
            string rawHash = $"partnerCode={_configuration["MomoAPI:PartnerCode"]}&accessKey={_configuration["MomoAPI:AccessKey"]}&requestId={requestId}&amount={amount.ToString()}&orderId={orderId}&orderInfo={orderInfo}&returnUrl={_configuration["MomoAPI:ReturnUrl"]}&notifyUrl={_configuration["MomoAPI:NotifyUrl"]}&extraData=";
            string secretKey = _configuration["MomoAPI:SecretKey"];
            
            string signatureCheck = DataEncryptionExtensions.SignSHA256(rawHash, secretKey);
            //string signatureFromRequest = signatureJSON.Signature;
            string signatureFromRequest = signatureCheck;
            Console.WriteLine($"Signature Check : {signatureCheck}");
            Console.WriteLine($"Signature from Request: {signatureFromRequest}");

            // Debug HERE if you get trouble LOL
            Console.WriteLine("----------------------------------------------------------------");
            await Console.Out.WriteLineAsync($"{orderType} || {orderInfo} || {transId} || {resultCode} || {message} || {payType} || {responseTime} || {extraData} || {signature}");

            if (signatureCheck != signatureFromRequest)
            {
                Console.WriteLine("Thành công 1 nửa");
                // Chữ ký không hợp lệ
                return View("PaymentFail");
            }

            if (resultCode == 0 && message == "")
            {
                // Đang test nên tạm thời chưa lưu vào database
                // Thanh toán thành công
                // Lưu vào database

                var response = new MOMOPaymentResponseModel
                {
                    OrderType = orderType,
                    OrderInfo = orderInfo,
                    TransId = transId,
                    ResultCode = requestId,
                    Message = message,
                    PayType = payType,
                    Amount = Decimal.Parse(amount),
                    ResponseTime = responseTime,
                    ExtraData = extraData,
                    Signature = signature
                };

                // Truy xuất đối tượng từ TempData
                //var momoModel = TempData.GetObjectFromJson<MOMOPaymentRequestModel>("MOMOPaymentRequestModel");
                //var appointment = new Appointment
                //{
                //    ScheduleID = momoModel.ScheduleID,
                //    PatientRecordID = momoModel.PatientRecordID,
                //    SpecialtyID = momoModel.SpecialtyID,
                //    TotalPrice = momoModel.Amount,
                //    CreatedDate = DateTime.Now,
                //    AppointmentStatus = "Chờ Xác Nhận"
                //};

                //_context.Appointments.Add(appointment);
                //_context.SaveChanges();

                //var transaction = new Transaction
                //{
                //    AppointmentID = appointment.ID,
                //    Date = DateTime.Now,
                //    BankName = response.OrderType,
                //    TransactionCode = response.TransId,
                //    PaymentMethod = "MOMO",
                //    TotalPrice = response.Amount,
                //    BankAccountNumber = "9704198526191432198",
                //    FullName = momoModel.FullName,
                //    Message = response.OrderInfo,
                //    Status = "Thành Công"
                //};

                //_context.Transactions.Add(transaction);
                //_context.SaveChanges();

                ViewBag.ResultCode = resultCode;
                ViewBag.Message = message.ToUpper();
                return View("PaymentResult");
            }
            else
            {
                // Thanh toán thất bại
                ViewBag.ResultCode = resultCode;
                ViewBag.Message = message.ToUpper();
                return View("PaymentResult");
            }
        }

        #region RefundPayment MOMO API
        //[HttpPost]
        //public async Task<IActionResult> RefundPayment(long amount = 10000, long transId = 4058788926, string description = "")
        //{
        //    string endpoint = _configuration["MomoAPI:MomoApiRefundUrl"];
        //    string partnerCode = _configuration["MomoAPI:PartnerCode"];
        //    string accessKey = _configuration["MomoAPI:AccessKey"];
        //    string secretKey = _configuration["MomoAPI:SecretKey"];
        //    string orderId = Guid.NewGuid().ToString();
        //    string requestId = Guid.NewGuid().ToString();
        //    string lang = "vi"; // or "vi"

        //    // Tạo chữ ký (signature)
        //    string rawHash = $"accessKey={accessKey}&amount={amount}&description=&orderId={orderId}&partnerCode={partnerCode}&requestId={requestId}&transId={transId}";

        //    string signature = DataEncryptionExtensions.SignSHA256(rawHash, secretKey);

        //    var refundRequest = new
        //    {
        //        partnerCode,
        //        orderId,
        //        requestId,
        //        amount,
        //        transId,
        //        lang,
        //        description,
        //        signature
        //    };

        //    string jsonRefundRequest = JsonConvert.SerializeObject(refundRequest);
        //    Console.WriteLine("JSON Request: " + jsonRefundRequest);
        //    Console.WriteLine("Raw Hash: " + rawHash);
        //    Console.WriteLine("Signature: " + signature);

        //    using (var client = new HttpClient())
        //    {
        //        var content = new StringContent(jsonRefundRequest, Encoding.UTF8, "application/json");
        //        var response = await client.PostAsync(endpoint, content);
        //        var responseString = await response.Content.ReadAsStringAsync();
        //        Console.WriteLine("JSON Response: " + responseString);

        //        var responseObject = JsonConvert.DeserializeObject<MOMORefundResponseModel>(responseString);

        //        if (responseObject != null && responseObject.resultCode == 0)
        //        {
        //            // Hoàn tiền thành công
        //            //return View("RefundSuccess", responseObject);
        //            return RedirectToAction("RefundSuccess", "payment");

        //        }
        //        else
        //        {
        //            // Xử lý lỗi
        //            //return View("RefundFail", responseObject);
        //            return RedirectToAction("RefundFail", "payment");
        //        }
        //    }
        //}
        #endregion



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


        //[Authorize(Roles = "Bệnh Nhân")]
        //public IActionResult RefundCallBack()
        //{
        //    var response = _vnPayment.RefundExecute(Request.Query);
        //    Console.WriteLine($"Response Code = {response.VnPayResponseCode}");

        //    if (response == null || response.VnPayResponseCode != "00")
        //    {
        //        TempData["Message"] = $"Lỗi hoàn tiền VN Pay: {response.VnPayResponseCode}";
        //        return RedirectToAction("RefundFail");
        //    }

        //    TempData["Message"] = $"Hoàn tiền VNPay thành công";
        //    return RedirectToAction("RefundSuccess");
        //}

        // Re-Fun VNPAY END
    }
}
