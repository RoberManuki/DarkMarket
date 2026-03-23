using System;
using System.Collections.Generic;

namespace CryptoMarket.Models
{
    public class PaymentRecord
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
        public string Address { get; set; } = "";
        public string? PaymentId { get; set; } 
        public string? PaymentMethod { get; set; } 
        public decimal Amount { get; set; }
        public bool IsPaid { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PaidAt { get; set; }

        public string? PrivateKey { get; set; }

        public int? DeliveryAgentId { get; set; }
        public DeliveryAgent? DeliveryAgent { get; set; }
        public int? EstimatedDeliveryDays { get; set; }

        public Product? Product { get; set; }
        public int? OrderId { get; set; }
        public OrderModel? Order { get; set; }
    }
}
