using System;
using System.Collections.Generic;
using DarkMarket.Enums;
using DarkMarket.Models;
using Microsoft.EntityFrameworkCore;

[Index(nameof(PaymentId), IsUnique = true)]
public class OrderModel
{
    public int Id { get; set; }

    public string BuyerId { get; set; } = "";
    public ApplicationUser? Buyer { get; set; }

    public string? SellerId { get; set; }
    public ApplicationUser? Seller { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsPaid { get; set; }
    public string? PaymentId { get; set; }
    public PaymentRecord? Payment { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pendente;
    public DateTime? DeliveredAt { get; set; }
    public DateTime? FinishedAt { get; set; }

    public bool IsDelivered { get; set; } = false;
    public bool FundsReleased { get; set; } = false;

    public bool DeliveryPendingApproval { get; set; } = false;

    public List<OrderMessage> Messages { get; set; } = new();
}