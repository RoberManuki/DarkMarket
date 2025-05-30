namespace DarkMarket.Services
{
    public interface IBitcoinPaymentService
    {
        Task<(string Address, string? PaymentId)> GenerateAddressAsync(decimal amount, string? orderId = null);
        Task<decimal> GetReceivedAmountAsync(string address);
        string Name { get; }
    }
}