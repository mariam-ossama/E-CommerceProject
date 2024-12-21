using E_Commerce.Data;
using E_Commerce.DTOs;
using E_Commerce.Models;
using E_Commerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


//TODO: Testing
[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly EmailService _emailService;

    public PaymentController(AppDbContext context, EmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    [HttpPost("pay")]
    public async Task<IActionResult> MakePayment([FromBody] PaymentRequestDTO paymentRequest)
    {
        var user = await _context.Users.FindAsync(paymentRequest.UserId);
        if (user == null) return NotFound("User not found.");

        var basketItems = _context.BasketItems
            .Include(b => b.Product) // Ensure Product details are loaded
            .Where(b => b.UserId == paymentRequest.UserId).ToList();

        if (!basketItems.Any()) return BadRequest("Basket is empty.");

        var totalAmount = basketItems.Sum(b => b.Product.ProductPrice * b.Quantity);

        // Handle payment methods
        if (paymentRequest.PaymentMethod == "Wallet")
        {
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == paymentRequest.UserId);
            if (wallet == null || wallet.Balance < totalAmount)
                return BadRequest("Insufficient wallet balance.");

            wallet.Balance -= totalAmount;
        }
        else if (paymentRequest.PaymentMethod == "Card")
        {
            // Integrate with a payment gateway for card transactions
            var cardPaymentResult = await ProcessCardPayment(paymentRequest.CardDetails, totalAmount);
            if (!cardPaymentResult)
                return BadRequest("Card payment failed.");
        }
        else if (paymentRequest.PaymentMethod == "Cash")
        {
            // For Cash payments, no additional processing is needed
            // You might want to add custom handling or logging if necessary
        }
        else
        {
            return BadRequest("Invalid payment method.");
        }

        var transaction = new Transaction
        {
            UserId = paymentRequest.UserId,
            Amount = totalAmount,
            PaymentMethod = paymentRequest.PaymentMethod,
            TransactionDate = DateTime.UtcNow
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // Generate receipt and send email
        var receipt = GenerateReceipt(user, basketItems, totalAmount, transaction.TransactionDate);
        await _emailService.SendEmailWithAttachmentAsync(
            user.Email,
            "Payment Receipt",
            "Please find your receipt attached.",
            receipt,
            "Receipt.pdf"
        );

        return Ok("Payment successful.");
    }

    private async Task<bool> ProcessCardPayment(CardDetailsDTO cardDetails, decimal amount)
    {
        // Placeholder for actual payment gateway integration
        // Example: Integrating Stripe or another provider
        try
        {
            // Simulate card processing (replace with actual API calls)
            if (string.IsNullOrWhiteSpace(cardDetails.CardNumber) ||
                cardDetails.ExpiryMonth <= 0 ||
                cardDetails.ExpiryYear <= 0 ||
                string.IsNullOrWhiteSpace(cardDetails.CVV))
            {
                return false;
            }

            // Assume payment gateway API is called here and returns success/failure
            return true; // Replace with actual result from gateway
        }
        catch (Exception ex)
        {
            // Log the error
            Console.WriteLine($"Card payment error: {ex.Message}");
            return false;
        }
    }

    private byte[] GenerateReceipt(User user, List<BasketItem> basketItems, decimal totalAmount, DateTime transactionDate)
    {
        var receiptService = new ReceiptService();
        return receiptService.GenerateReceipt(user, basketItems, totalAmount, transactionDate);
    }

    [HttpGet("{userId}/wallet")]
    public async Task<IActionResult> GetWalletBalance(string userId)
    {
        var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        if (wallet == null) return NotFound("Wallet not found.");

        return Ok(new { Balance = wallet.Balance });
    }

    [HttpPost("{userId}/wallet/add")]
    public async Task<IActionResult> AddFunds(string userId, [FromBody] decimal amount)
    {
        var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        if (wallet == null) return NotFound("Wallet not found.");

        wallet.Balance += amount;
        await _context.SaveChangesAsync();

        return Ok("Funds added successfully.");
    }

    [HttpPost("cancel-order")]
    [Authorize]
    public async Task<IActionResult> CancelOrder([FromBody] CancelOrderDTO cancelOrderRequest)
    {
        var order = await _context.Orders.Include(o => o.OrderItems)
                                         .FirstOrDefaultAsync(o => o.OrderId == cancelOrderRequest.OrderId && o.UserId == cancelOrderRequest.UserId);

        if (order == null) return NotFound("Order not found.");

        if (!order.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only completed orders can be canceled.");

        // Roll back the payment
        var transaction = await _context.Transactions.FirstOrDefaultAsync(t => t.UserId == order.UserId && t.Amount == order.TotalAmount);

        if (transaction == null) return NotFound("Transaction not found.");

        if (transaction.PaymentMethod == "Wallet")
        {
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == order.UserId);
            if (wallet == null) return NotFound("Wallet not found.");

            wallet.Balance += order.TotalAmount;
        }
        else if (transaction.PaymentMethod == "Card")
        {
            // Add logic to initiate a refund through the card payment gateway
            await RefundCardPayment(transaction);
        }

        // Update order status to "Cancelled"
        order.Status = "Cancelled";
        _context.Orders.Update(order);

        await _context.SaveChangesAsync();
        return Ok("Order cancelled and payment refunded successfully.");
    }

    private async Task<bool> RefundCardPayment(Transaction transaction)
    {
        // Logic to refund the payment via card payment gateway
        try
        {
            // Simulate refund processing
            return true; // Replace with actual refund gateway integration
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Card refund error: {ex.Message}");
            return false;
        }
    }

}
