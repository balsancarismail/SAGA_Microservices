namespace Shared.Messages;

public class OrderItemMessage
{
    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }
}