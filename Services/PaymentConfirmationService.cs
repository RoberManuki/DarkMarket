using DarkMarket.Data;
using DarkMarket.Models;
using Microsoft.EntityFrameworkCore;

namespace DarkMarket.Services
{
    public class PaymentConfirmationService
    {
        private readonly AppDbContext _db;
        private readonly BitcoinPaymentFactory _paymentFactory;
        private readonly LogService _logService;

        public PaymentConfirmationService(
            AppDbContext db,
            BitcoinPaymentFactory paymentFactory,
            LogService logService)
        {
            _db = db;
            _paymentFactory = paymentFactory;
            _logService = logService;
        }

        public async Task<(bool Confirmed, bool AlreadyPaid, decimal ReceivedAmount)> ConfirmAsync(PaymentRecord payment)
        {
            var dbPayment = await _db.Payments
                .Include(p => p.Product)
                .FirstOrDefaultAsync(p => p.Id == payment.Id);

            if (dbPayment == null)
                return (false, false, 0m);

            if (dbPayment.IsPaid)
                return (true, true, dbPayment.Amount);

            var paymentMethod = dbPayment.PaymentMethod ?? "Testnet";
            var service = _paymentFactory.GetService(paymentMethod);
            var parameter = paymentMethod == "Testnet"
                ? dbPayment.Address
                : dbPayment.PaymentId ?? dbPayment.Address;

            var received = await service.GetReceivedAmountAsync(parameter);
            if (received < dbPayment.Amount)
                return (false, false, received);

            dbPayment.IsPaid = true;
            dbPayment.PaidAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            if (service is TestnetBitcoinPaymentService testnetService && !string.IsNullOrEmpty(dbPayment.PaymentId))
            {
                await testnetService.CheckAndMarkPaymentAsync(_db, _logService, dbPayment.PaymentId);
            }

            await _logService.LogAsync(
                $"Pagamento confirmado para paymentId={dbPayment.PaymentId} via {paymentMethod}.",
                source: "Payment",
                level: "Info",
                userId: dbPayment.UserId
            );

            return (true, false, received);
        }
    }
}