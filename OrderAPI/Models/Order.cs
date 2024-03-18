namespace OrderAPI.Models;

public class Order
{
    public int Id { get; set; }

    public int BuyerId { get; set; }

    public OrderStatus OrderStatus { get; set; }
    public decimal TotalPrice { get; set; }

    public DateTime CreatedDate { get; set; }

    public IList<OrderItem> OrderItems { get; set; }
}