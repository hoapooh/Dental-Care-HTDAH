namespace Dental_Clinic_System.Services.MOMO
{
    public interface IMOMOPayment
    {
        Task<string> CreateMOMOPayment(MOMOPaymentRequestModel model);
        Task<MOMORefundResponseModel?> RefundPayment(long amount, long transId, string description);
    }
}
