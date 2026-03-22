using DarkMarket.Enums;
using DarkMarket.Shared.Helpers;

namespace DarkMarket.Tests;

public class OrderStatusHelperTests
{
    [Theory]
    [InlineData(PaymentStatus.AguardandoPagamento, "Aguardando pagamento")]
    [InlineData(PaymentStatus.Pago, "Pago")]
    [InlineData(PaymentStatus.AguardandoEntrega, "Aguardando entrega")]
    [InlineData(PaymentStatus.Entregue, "Entregue")]
    [InlineData(PaymentStatus.Finalizado, "Finalizada")]
    [InlineData(PaymentStatus.Cancelado, "Cancelado")]
    [InlineData(PaymentStatus.Disputa, "Em disputa")]
    [InlineData(PaymentStatus.AguardandoRevisaoAdm, "Aguardando revisão ADM")]
    [InlineData(PaymentStatus.Pendente, "Pendente")]
    [InlineData(PaymentStatus.Reembolsado, "Reembolsado")]
    [InlineData(PaymentStatus.Falha, "Falha")]
    public void GetStatusText_ReturnsExpectedText_ForKnownStatuses(PaymentStatus status, string expected)
    {
        var result = OrderStatusHelper.GetStatusText(status);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetStatusText_ReturnsOutro_ForUnknownStatus()
    {
        var unknown = (PaymentStatus)999;

        var result = OrderStatusHelper.GetStatusText(unknown);

        Assert.Equal("Outro", result);
    }
}
