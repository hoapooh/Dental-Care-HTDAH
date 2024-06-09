namespace Dental_Clinic_System.Services.VNPAY
{
    public class VNPaymentResponseModel
    {
        public bool Success { get; set; }
        public string PaymentMethod { get; set; }
        public string OrderDescription { get; set; }
        public string OrderId { get; set; }
        public string PaymentId { get; set; }
        public string TransactionId { get; set; }
        public string Token { get; set; }
        public string VnPayResponseCode { get; set; }
        public string? Message { get; set; } = "Hello World";
    }

    public class VNPaymentRequestModel()
    {
        public int DepositID { get; set; }
        public string FullName { get; set; }
        public string Description { get; set; }
        public double Amount { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class VNPaymentRefundRequestModel
    {
        public string RequestId { get; set; } // Unique request ID
        public string TransactionId { get; set; }
        public long Amount { get; set; } // Amount to be refunded
        public string OrderInfo { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreateBy { get; set; } // User who initiates the refund
    }

}
