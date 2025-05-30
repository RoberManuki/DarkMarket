namespace DarkMarket.Services
{
    public class LightningPaymentService : IBitcoinPaymentService
    {
        public string Name => "Lightning";

        public Task<(string Address, string? PaymentId)> GenerateAddressAsync(decimal amount, string? orderId = null)
        {
            // Aqui você geraria um invoice Lightning real
            return Task.FromResult<(string, string?)>(("lnbc1...", "invoiceId123"));
        }

        public Task<decimal> GetReceivedAmountAsync(string address)
        {
            // Aqui você consultaria o status do invoice Lightning
            return Task.FromResult(0m);
        }
    }
}