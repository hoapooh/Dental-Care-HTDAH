
using static Dental_Clinic_System.Services.VNPAY.VNPAYLibrary;
using System.Security.Policy;
using System.Globalization;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using System.Net;

namespace Dental_Clinic_System.Services.VNPAY
{
    public class VNPayment : IVNPayment
    {
        private readonly IConfiguration _configuration;
        //private readonly HttpClient _httpClient;

        public VNPayment(IConfiguration config)
        {
            _configuration = config;
            //_httpClient = httpClient;
        }

        public string CreatePaymentURL(HttpContext context, VNPaymentRequestModel model)
        {
            var tick = DateTime.Now.Ticks.ToString();

            var vnpay = new VNPAYLibrary();

            vnpay.AddRequestData("vnp_Version", _configuration["VNPAY:Version"]);
            vnpay.AddRequestData("vnp_Command", _configuration["VNPAY:Command"]);
            vnpay.AddRequestData("vnp_TmnCode", _configuration["VNPAY:TmnCode"]);
            vnpay.AddRequestData("vnp_Amount", (model.Amount * 100).ToString()); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần(khử phần thập phân), sau đó gửi sang VNPAY là: 10000000

            //if (cboBankCode.SelectedItem != null && !string.IsNullOrEmpty(cboBankCode.SelectedItem.Value))
            //{
            //    vnpay.AddRequestData("vnp_BankCode", cboBankCode.SelectedItem.Value);
            //}
            vnpay.AddRequestData("vnp_CreateDate", model.CreatedDate.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", _configuration["VNPAY:CurrencyCode"]);
            vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(context));
            vnpay.AddRequestData("vnp_Locale", _configuration["VNPAY:Locale"]);
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toán đặt cọc: " + model.DepositID);
            //vnpay.AddRequestData("vnp_OrderType", orderCategory.SelectedItem.Value); //default value: other
            vnpay.AddRequestData("vnp_OrderType", "other"); //default value: other
            vnpay.AddRequestData("vnp_ReturnUrl", _configuration["VNPAY:PaymentCallBackURL"]);
            vnpay.AddRequestData("vnp_TxnRef", tick); // Mã tham chiếu của giao dịch tại hệ thống của merchant.Mã này là duy nhất dùng để phân biệt các đơn hàng gửi sang VNPAY.Không được trùng lặp trong ngày

            var paymentURL = vnpay.CreateRequestUrl(_configuration["VNPAY:BaseUrl"], _configuration["VNPAY:HashSecret"]);

            return paymentURL;
        }

        public VNPaymentResponseModel PaymentExecute(IQueryCollection collection)
        {
            var vnpay = new VNPAYLibrary();
            foreach (var (key, value) in collection)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value.ToString());
                    Console.WriteLine($"Key = {key} | Value = {value.ToString()}");
                }
            }

            var vnp_orderID = Convert.ToInt64(vnpay.GetResponseData("vnp_TxnRef"));
            var vnp_transactionID = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));
            var vnp_secureHash = collection.FirstOrDefault(e => e.Key == "vnp_SecureHash").Value;
            var vnp_responseCode = vnpay.GetResponseData("vnp_ResponseCode");
            var vnp_orderInfo = vnpay.GetResponseData("vnp_OrderInfo");

            var vnp_amount = vnpay.GetResponseData("vnp_Amount");
            Console.WriteLine($"Amount = {vnp_amount} | Deposit Info = {vnp_orderInfo}");

            bool checkSignature = vnpay.ValidateSignature(vnp_secureHash, _configuration["VNPAY:HashSecret"]);
            if (!checkSignature)
            {
                return new VNPaymentResponseModel
                {
                    Success = false,
                };
            }

            return new VNPaymentResponseModel
            {
                Success = true,
                PaymentMethod = "VNPAY",
                OrderDescription = vnp_orderInfo,
                OrderId = vnp_orderID.ToString(),
                TransactionId = vnp_transactionID.ToString(),
                Token = vnp_secureHash,
                VnPayResponseCode = vnp_responseCode
            };
        }

        public string CreateRefundURL(HttpContext context, VNPaymentRefundRequestModel model)
        {
            var tick = DateTime.Now.Ticks.ToString();

            var vnpay = new VNPAYLibrary();

            vnpay.AddRequestData("vnp_RequestId", model.RequestId); // oke
            vnpay.AddRequestData("vnp_Version", _configuration["VNPAY:Version"]); // oke
            vnpay.AddRequestData("vnp_Command", _configuration["VNPAY:RefundCommand"]); // oke
            vnpay.AddRequestData("vnp_TmnCode", _configuration["VNPAY:TmnCode"]); // oke
            vnpay.AddRequestData("vnp_TransactionType", "02"); // Transaction type 02 for full refund // oke
            vnpay.AddRequestData("vnp_TxnRef", tick); // oke
            vnpay.AddRequestData("vnp_Amount", (model.Amount * 100).ToString()); // oke
            vnpay.AddRequestData("vnp_OrderInfo", "Hoàn tiền cho giao dịch " + model.RequestId); // oke
            vnpay.AddRequestData("vnp_TransactionNo", model.TransactionId); // oke
            vnpay.AddRequestData("vnp_TransactionDate", model.TransactionDate.ToString("yyyyMMddHHmmss")); // oke
            vnpay.AddRequestData("vnp_CreateDate", model.CreatedDate.ToString("yyyyMMddHHmmss")); // oke
            vnpay.AddRequestData("vnp_CreateBy", model.CreateBy); // oke
            vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(context)); // oke

            var refundURL = vnpay.CreateRequestUrl(_configuration["VNPAY:RefundURL"], _configuration["VNPAY:HashSecret"]);

            return refundURL;
        }

        public VNPaymentResponseModel RefundExecute(IQueryCollection collection)
        {
            var vnpay = new VNPAYLibrary();
            foreach (var (key, value) in collection)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value.ToString());
                    Console.WriteLine($"Key = {key} | Value = {value.ToString()}");
                }
            }

            var vnp_secureHash = collection.FirstOrDefault(e => e.Key == "vnp_SecureHash").Value;
            //var vnp_responseCode = vnpay.GetResponseData("vnp_ResponseCode");
            var vnp_ResponseId = vnpay.GetResponseData("vnp_ResponseId");
            var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            var vnp_Message = vnpay.GetResponseData("vnp_Message");
            var vnp_TransactionNo = vnpay.GetResponseData("vnp_TransactionNo");
            var vnp_TransactionType = vnpay.GetResponseData("vnp_TransactionType");
            var vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
            var vnp_OrderInfo = vnpay.GetResponseData("vnp_OrderInfo");
            var vnp_BankCode = vnpay.GetResponseData("vnp_BankCode");
            var vnp_PayDate = vnpay.GetResponseData("vnp_PayDate");
            var vnp_orderID = Convert.ToInt64(vnpay.GetResponseData("vnp_TxnRef"));

            bool checkSignature = vnpay.ValidateSignature(vnp_secureHash, _configuration["VNPAY:HashSecret"]);
            if (!checkSignature)
            {
                return new VNPaymentResponseModel
                {
                    Success = false,
                };
            }

            return new VNPaymentResponseModel
            {
                Success = true,
                VnPayResponseCode = vnp_ResponseCode,
                //Message = vnp_Message,
                OrderDescription = vnp_OrderInfo.ToString(),
                OrderId = vnp_orderID.ToString(),
                TransactionId = vnp_TransactionNo.ToString(),
                Token = vnp_secureHash

            };
        }

        //public async Task<Dictionary<string, string>> CreateRefundURL(HttpContext context, VNPaymentRefundRequestModel model)
        //{
        //    var vnpay = new VNPAYLibrary();

        //    vnpay.AddRequestData("vnp_RequestId", model.RequestId); // Unique request ID
        //    vnpay.AddRequestData("vnp_Version", _configuration["VNPAY:Version"]); // API version
        //    vnpay.AddRequestData("vnp_Command", _configuration["VNPAY:RefundCommand"]); // Refund command
        //    vnpay.AddRequestData("vnp_TmnCode", _configuration["VNPAY:TmnCode"]); // Terminal code
        //    vnpay.AddRequestData("vnp_TransactionType", "02"); // Full refund
        //    vnpay.AddRequestData("vnp_TxnRef", model.TransactionId); // Transaction reference
        //    vnpay.AddRequestData("vnp_Amount", (model.Amount * 100).ToString()); // Amount in VND (multiplying by 100)
        //    vnpay.AddRequestData("vnp_OrderInfo", model.OrderInfo); // Order info
        //    vnpay.AddRequestData("vnp_TransactionNo", model.TransactionId); // Transaction number
        //    vnpay.AddRequestData("vnp_TransactionDate", model.TransactionDate.ToString("yyyyMMddHHmmss")); // Transaction date
        //    vnpay.AddRequestData("vnp_CreateDate", model.CreatedDate.ToString("yyyyMMddHHmmss")); // Create date
        //    vnpay.AddRequestData("vnp_CreateBy", model.CreateBy); // Created by
        //    vnpay.AddRequestData("vnp_IpAddr", VNPAYLibrary.Utils.GetIpAddress(context)); // IP address

        //    // Generate secure hash
        //    var requestData = vnpay.GetRequestData();
        //    var data = string.Join("|", requestData.Select(kvp => kvp.Value));
        //    var hashSecret = _configuration["VNPAY:HashSecret"];
        //    var secureHash = VNPAYLibrary.Utils.GenerateSecureHash(data, hashSecret);
        //    vnpay.AddRequestData("vnp_SecureHash", secureHash);

        //    // Log the request data for debugging
        //    foreach (var item in requestData)
        //    {
        //        Console.WriteLine($"Key: {item.Key}, Value: {item.Value}");
        //    }

        //    var requestUrl = _configuration["VNPAY:RefundURL"];
        //    var jsonContent = JsonConvert.SerializeObject(requestData);
        //    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        //    var response = await _httpClient.PostAsync(requestUrl, content);
        //    var responseString = await response.Content.ReadAsStringAsync();

        //    // Log the response for debugging
        //    Console.WriteLine($"Refund Response: {responseString}");

        //    // Parse the response as a query string safely
        //    var responseData = responseString.Split('&')
        //        .Select(param => param.Split('='))
        //        .Where(kv => kv.Length == 2)
        //        .ToDictionary(kv => kv[0], kv => WebUtility.UrlDecode(kv[1]));

        //    return responseData;
        //}

        //public VNPaymentResponseModel RefundExecute(IQueryCollection collection)
        //{
        //    var vnpay = new VNPAYLibrary();
        //    foreach (var (key, value) in collection)
        //    {
        //        if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
        //        {
        //            vnpay.AddResponseData(key, value.ToString());
        //            Console.WriteLine($"Key = {key} | Value = {value.ToString()}");
        //        }
        //    }

        //    var vnp_secureHash = collection.FirstOrDefault(e => e.Key == "vnp_SecureHash").Value;
        //    var vnp_ResponseId = vnpay.GetResponseData("vnp_ResponseId");
        //    var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
        //    var vnp_Message = vnpay.GetResponseData("vnp_Message");
        //    var vnp_TransactionNo = vnpay.GetResponseData("vnp_TransactionNo");
        //    var vnp_TransactionType = vnpay.GetResponseData("vnp_TransactionType");
        //    var vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
        //    var vnp_OrderInfo = vnpay.GetResponseData("vnp_OrderInfo");
        //    var vnp_BankCode = vnpay.GetResponseData("vnp_BankCode");
        //    var vnp_PayDate = vnpay.GetResponseData("vnp_PayDate");
        //    var vnp_orderID = vnpay.GetResponseData("vnp_TxnRef");

        //    bool checkSignature = vnpay.ValidateSignature(vnp_secureHash, _configuration["VNPAY:HashSecret"]);
        //    if (!checkSignature)
        //    {
        //        return new VNPaymentResponseModel
        //        {
        //            Success = false,
        //        };
        //    }

        //    return new VNPaymentResponseModel
        //    {
        //        Success = true,
        //        VnPayResponseCode = vnp_ResponseCode,
        //        Message = vnp_Message,
        //        OrderDescription = vnp_OrderInfo,
        //        OrderId = vnp_orderID,
        //        TransactionId = vnp_TransactionNo,
        //        Token = vnp_secureHash
        //    };
        //}



    }
}
