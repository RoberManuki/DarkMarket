using System.Text;
using Microsoft.AspNetCore.Http;

namespace DarkMarket.Tests;

internal static class WebhookTestFactory
{
    public static DefaultHttpContext CreateContext(string secret, string invoiceId, string eventType)
    {
        var body = $"{{\"invoiceId\":\"{invoiceId}\",\"type\":\"{eventType}\"}}";
        return CreateContextRaw(secret, body);
    }

    public static DefaultHttpContext CreateContextRaw(string secret, string rawBody)
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-BTCPay-Secret"] = secret;
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(rawBody));
        context.Request.ContentLength = context.Request.Body.Length;
        context.Request.Body.Position = 0;
        return context;
    }
}