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

namespace Dental_Clinic_System.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IVNPayment _vnPayment;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public PaymentController(IVNPayment vnPayment, IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _vnPayment = vnPayment;
            _httpClientFactory = httpClientFactory;
            _configuration = config;
        }

        [Authorize(Roles = "Bệnh Nhân")]
        public IActionResult Index()
        {
            var vnpayModel = new VNPaymentRequestModel
            {
                Amount = 20000,
                CreatedDate = DateTime.Now,
                FullName = "Đỗ Anh Tú",
                DepositID = 0,
            };
            return Redirect(_vnPayment.CreatePaymentURL(HttpContext, vnpayModel));
        }

        [HttpPost]
        [Authorize(Roles = "Bệnh Nhân")]
        public async Task<IActionResult> ProcessRefund(string txnRef = "638536559852904902", long amount = 20000)
        {
            var requestId = Guid.NewGuid().ToString();
            var version = _configuration["VNPAY:Version"];
            var command = _configuration["VNPAY:RefundCommand"];
            var tmnCode = _configuration["VNPAY:TmnCode"];
            var transactionType = "02"; // hoặc "03" cho hoàn trả một phần
            var orderInfo = "Refund for order";
            var transactionDate = DateTime.Now.ToString("yyyyMMddHHmmss");
            var createBy = "admin";
            var createDate = DateTime.Now.ToString("yyyyMMddHHmmss");
            var ipAddr = HttpContext.Connection.RemoteIpAddress.ToString();
            var secureHash = _configuration["VNPAY:HashSecret"];

            // Tạo chuỗi dữ liệu để tính checksum
            string data = $"{requestId}|{version}|{command}|{tmnCode}|{transactionType}|{txnRef}|{amount}|{transactionDate}|{createBy}|{createDate}|{ipAddr}|{orderInfo}";

            // Tính checksum
            string secureHashed = ComputeSHA256Hash(secureHash, data);

            var jsonData = new JObject
            {
                ["vnp_RequestId"] = requestId,
                ["vnp_Version"] = version,
                ["vnp_Command"] = command,
                ["vnp_TmnCode"] = tmnCode,
                ["vnp_TransactionType"] = transactionType,
                ["vnp_TxnRef"] = txnRef,
                ["vnp_Amount"] = amount.ToString(),
                ["vnp_OrderInfo"] = orderInfo,
                ["vnp_TransactionDate"] = transactionDate,
                ["vnp_CreateBy"] = createBy,
                ["vnp_CreateDate"] = createDate,
                ["vnp_IpAddr"] = ipAddr,
                ["vnp_SecureHash"] = secureHashed
            };

            var jsonString = JsonConvert.SerializeObject(jsonData);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            Console.WriteLine("JSON STRING: " + jsonString);

            using (var client = _httpClientFactory.CreateClient())
            {
                var response = await client.PostAsync(_configuration["VNPAY:RefundURL"], content);
                var responseString = await response.Content.ReadAsStringAsync();
                return ProcessResponse(responseString);
            }
        }

        private IActionResult ProcessResponse(string responseString)
        {
            var jsonResponse = JObject.Parse(responseString);
            Console.WriteLine(jsonResponse.ToString());
            var responseCode = jsonResponse["vnp_ResponseCode"].ToString();
            Console.WriteLine($"Response Code = {responseCode}");
            if (responseCode == "00")
            {
                return RedirectToAction("RefundSuccess");
            }
            else
            {
                return RedirectToAction("RefundFail");
            }
        }

        public static string ComputeSHA256Hash(string secretKey, string rawData)
        {
            using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                byte[] bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        [Authorize(Roles = "Bệnh Nhân")]
        public IActionResult PaymentCallBack()
        {
            var response = _vnPayment.PaymentExecute(Request.Query);
            Console.WriteLine($"Response Code = {response.VnPayResponseCode}");

            if (response == null || response.VnPayResponseCode != "00")
            {
                TempData["Message"] = $"Lỗi thanh toán VN Pay: {response.VnPayResponseCode}";
                return RedirectToAction("PaymentFail");
            }

            // Lưu đơn hàng vô database

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
    }
}
