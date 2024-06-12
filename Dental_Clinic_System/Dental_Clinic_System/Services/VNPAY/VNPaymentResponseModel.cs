﻿using Dental_Clinic_System.Helper;

namespace Dental_Clinic_System.Services.VNPAY
{
    public class VNPaymentResponseModel
    {
        public bool Success { get; set; }
        public string PaymentMethod { get; set; }
        public string OrderDescription { get; set; }
        public string OrderId { get; set; }
        public string TransactionId { get; set; }
        public string Token { get; set; }
        public string VnPayResponseCode { get; set; }
        public string BankCode { get; set; }
        public string CardType { get; set; }
        public decimal Amount { get; set; }
        public string BankTransactionNo { get; set; }
        public DateTime CreatedDate { get; set; }
		public string? Message { get; set; } = "Hello World";
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
