using System;
using DarkMarket.Models;

public class OrderMessage
{
    public int Id { get; set; }
    public int PaymentId { get; set; }
    public PaymentRecord? Payment { get; set; }
    public string? SenderUserId { get; set; }
    public ApplicationUser? Sender { get; set; }
    public string? Content { get; set; }
    public DateTime SentAt { get; set; }
}