namespace Dental_Clinic_System.Services.MOMO
{
	public class MOMOPaymentResponseModel
	{
        //public string partnerCode { get; set; } = string.Empty;
        //public string orderId { get; set; } = string.Empty;
        //public string requestId { get; set; } = string.Empty;
        //public long amount { get; set; }
        public string OrderInfo {  get; set; }
        public string OrderType { get; set; }
        public string TransId { get; set; }
        public string ResultCode { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string PayType { get; set; }
        public long ResponseTime { get; set; }
        public string ExtraData { get; set; }
        public string Signature { get; set; }
        public string payUrl { get; set; } = string.Empty;
        public decimal Amount { get; internal set; }
        //public string deeplink { get; set; } = string.Empty;
        //public string qrCodeUrl { get; set; } = string.Empty;
    }
}
