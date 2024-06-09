using Dental_Clinic_System.Services.VNPAY;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

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

        //[Authorize(Roles = "Bệnh Nhân")]
        //public IActionResult Refund(string transactionId, long amount, string transactionDate, string createBy)
        //{
        //    var refundModel = new VNPaymentRefundRequestModel
        //    {
        //        RequestId = Guid.NewGuid().ToString(), // Unique request ID
        //        TransactionId = transactionId,
        //        Amount = amount,
        //        OrderInfo = "Hoàn tiền cho giao dịch " + transactionId,
        //        TransactionDate = transactionDate,
        //        CreateBy = createBy // Use the specified user name or identifier
        //    };

        //    return Redirect(_vnPayment.CreateRefundURL(HttpContext, refundModel));
        //}

        //[HttpPost]
        ////[Authorize(Roles = "Bệnh Nhân")]
        //public IActionResult Refund()
        //{
        //    var refundModel = new VNPaymentRefundRequestModel
        //    {
        //        RequestId = Guid.NewGuid().ToString(), // Unique request ID
        //        TransactionId = "14450704",
        //        Amount = 20000000,
        //        OrderInfo = "Hoàn tiền cho giao dịch 14450704",
        //        TransactionDate = DateTime.Now, // Format as string
        //        CreatedDate= DateTime.Now,
        //        CreateBy = "Đỗ Anh Tú" // Use the specified user name or identifier
        //    };

        //    // Log the refund model data for debugging
        //    Console.WriteLine($"RequestId = {refundModel.RequestId}");
        //    Console.WriteLine($"TransactionId = {refundModel.TransactionId}");
        //    Console.WriteLine($"Amount = {refundModel.Amount}");
        //    Console.WriteLine($"OrderInfo = {refundModel.OrderInfo}");
        //    Console.WriteLine($"TransactionDate = {refundModel.TransactionDate}");
        //    Console.WriteLine($"CreateBy = {refundModel.CreateBy}");

        //    var refundURL = _vnPayment.CreateRefundURL(HttpContext, refundModel);
        //    return Redirect(refundURL);
        //}



        private string CreateChecksum(string data, string secretKey)
        {
            using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secretKey)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Refund(string txnRef = "14450898", long amount = 20000 * 100)
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

            //var data = $"{requestId}|{version}|{command}|{tmnCode}|{transactionType}|{txnRef}|{amount}||{transactionDate}|{createBy}|{createDate}|{ipAddr}|{orderInfo}";
            //var secureHash = CreateChecksum(data, secretKey);

            var jsonData = new JObject
            {
                ["vnp_RequestId"] = requestId,
                ["vnp_Version"] = version,
                ["vnp_Command"] = command,
                ["vnp_TmnCode"] = tmnCode,
                ["vnp_TransactionType"] = transactionType,
                ["vnp_TxnRef"] = txnRef,
                ["vnp_Amount"] = amount,
                ["vnp_OrderInfo"] = orderInfo,
                ["vnp_TransactionDate"] = transactionDate,
                ["vnp_CreateBy"] = createBy,
                ["vnp_CreateDate"] = createDate,
                ["vnp_IpAddr"] = ipAddr,
                ["vnp_SecureHash"] = secureHash
            };

            using (var client = new HttpClient())
            {
                var content = new StringContent(jsonData.ToString(), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(_configuration["VNPAY:RefundURL"], content);
                var responseString = await response.Content.ReadAsStringAsync();
                return ProcessResponse(responseString);
            }
        }

        private IActionResult ProcessResponse(string responseString)
        {
            var jsonResponse = JObject.Parse(responseString);
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





        //[Authorize(Roles = "Bệnh Nhân")]
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
