namespace E_Commerce.Models
{
    public class Order
{
    public int OrderId { get; set; }
    public string UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } // "Completed", "In Progress", "Cancelled"
    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; }
}
}
