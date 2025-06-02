using System;
using DarkMarket.Models;

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
}