﻿using Microsoft.Extensions.Configuration.UserSecrets;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Text;
using Dental_Clinic_System.Helper;
using Microsoft.AspNetCore.Mvc;
using Dental_Clinic_System.Models.Data;

namespace Dental_Clinic_System.Services.MOMO
{
	public class MOMOPayment : IMOMOPayment
    {
        private readonly IConfiguration _configuration;
        private readonly DentalClinicDbContext _context;

        public MOMOPayment(IConfiguration config, DentalClinicDbContext context)
        {
            _configuration = config;
            _context = context;
        }

        [HttpPost]
        public async Task<string> CreateMOMOPayment(MOMOPaymentRequestModel model)
        {
            string endpoint = _configuration["MomoAPI:MomoApiUrl"];
            string partnerCode = _configuration["MomoAPI:PartnerCode"];
            string accessKey = _configuration["MomoAPI:AccessKey"];
            string secretKey = _configuration["MomoAPI:SecretKey"];
            string orderInfo = model.OrderInformation;
            string returnUrl = _configuration["MomoAPI:ReturnUrl"];
            string notifyUrl = _configuration["MomoAPI:NotifyUrl"];
            string requestType = _configuration["MomoAPI:RequestType"];
            string amount = model.Amount.ToString(); // Số tiền thanh toán
            string orderId = model.OrderID;
            string requestId = orderId;
            string extraData = "";

            // Tạo chữ ký (signature)
            string rawHash = $"partnerCode={partnerCode}&accessKey={accessKey}&requestId={requestId}&amount={amount.ToString()}&orderId={orderId}&orderInfo={orderInfo}&returnUrl={returnUrl}&notifyUrl={notifyUrl}&extraData=";

            string signature = DataEncryptionExtensions.SignSHA256(rawHash, secretKey);
            //var signatureJson = JsonConvert.SerializeObject(new SignatureModelForJSON { Signature = signature });
            await Console.Out.WriteLineAsync("----------------------------------");
            await Console.Out.WriteLineAsync($"Signature from MOMOPAYMENT = {signature}");

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
                    var paymentURL = responseObject.payUrl;
                    return paymentURL;
                }
                else
                {
                    Console.WriteLine("Fail!!!");
                    // Xử lý lỗi
                    return "PaymentFail";
                }
            }
        }
    }
}
