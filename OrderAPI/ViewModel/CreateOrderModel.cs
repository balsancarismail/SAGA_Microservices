using OrderAPI.Models;

namespace OrderAPI.ViewModel;

public class CreateOrderModel
{
    public int BuyerId { get; set; }

    public decimal TotalPrice { get; set; }

    public DateTime CreatedDate { get; set; }

    public IList<OrderItemModel> OrderItemModels { get; set; }
}