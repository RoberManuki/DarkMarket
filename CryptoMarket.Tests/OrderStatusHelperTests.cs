using CryptoMarket.Enums;
using CryptoMarket.Services;
using CryptoMarket.Shared.Helpers;

namespace CryptoMarket.Tests;

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
    [InlineData(PaymentStatus.AguardandoRevisaoAdm, "Aguardando revisao ADM")]
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

    [Theory]
    [InlineData("en-US", PaymentStatus.AguardandoRevisaoAdm, "Awaiting admin review")]
    [InlineData("es-ES", PaymentStatus.Finalizado, "Finalizada")]
    [InlineData("pt-BR", PaymentStatus.Disputa, "Em disputa")]
    public void GetStatusText_WithUiTextService_ReturnsLocalizedValue(string languageCode, PaymentStatus status, string expected)
    {
        var language = new LanguagePreferenceService();
        language.SetLanguage(languageCode);
        var textService = new UiTextService(language);

        var result = OrderStatusHelper.GetStatusText(status, textService);

        Assert.Equal(expected, result);
    }
}

