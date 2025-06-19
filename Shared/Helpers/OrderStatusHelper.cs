using DarkMarket.Enums;

namespace DarkMarket.Shared.Helpers
{
    public static class OrderStatusHelper
    {
        public static string GetStatusText(PaymentStatus status)
        {
            return status switch
            {
                PaymentStatus.AguardandoPagamento => "Aguardando pagamento",
                PaymentStatus.Pago => "Pago",
                PaymentStatus.AguardandoEntrega => "Aguardando entrega",
                PaymentStatus.Entregue => "Entregue",
                PaymentStatus.Finalizado => "Finalizada",
                PaymentStatus.Cancelado => "Cancelado",
                PaymentStatus.Disputa => "Em disputa",
                _ => "Outro"
            };
        }
    }
}