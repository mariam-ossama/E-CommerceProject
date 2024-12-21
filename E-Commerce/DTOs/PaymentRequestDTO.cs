namespace E_Commerce.DTOs
{
    public class PaymentRequestDTO
    {
        public string UserId { get; set; }
        public string PaymentMethod { get; set; } // "Wallet", "Card"
        public CardDetailsDTO? CardDetails { get; set; } // Nullable to allow non-card payments
    }

    public class CardDetailsDTO
    {
        public string CardNumber { get; set; }
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public string CVV { get; set; }
    }

    public class CancelOrderDTO
    {
        public int OrderId { get; set; }
        public string UserId { get; set; }
    }
}
