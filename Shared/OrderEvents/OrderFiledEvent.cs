namespace Shared.OrderEvents;

public class OrderFiledEvent
{
    public int OrderId { get; set; }
    public string Message { get; set; }
}