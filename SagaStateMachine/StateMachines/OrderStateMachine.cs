using MassTransit;
using SagaStateMachine.StateInstances;
using Shared;
using Shared.Messages;
using Shared.OrderEvents;
using Shared.PaymentEvents;
using Shared.StockEvents;

namespace SagaStateMachine.StateMachines;

public class OrderStateMachine : MassTransitStateMachine<OrderStateInstance>
{

    public Event<OrderStartedEvent> OrderStartedEvent { get; private set; }
    public Event<StockReservedEvent> StockReservedEvent { get; set; }
    public Event<StockNotReservedEvent> StockNotReservedEvent { get; set; }
    public Event<PaymentCompletedEvent> PaymentCompletedEvent { get; set; }
    public Event<PaymentFailedEvent> PaymentFailedEvent { get; set; }

    public State OrderCreated { get; set; }
    public State StockReserved { get; set; }
    public State StockNotReserved { get; set; }
    public State PaymentCompleted { get; set; }
    public State PaymentFailed { get; set; }

    public OrderStateMachine()
    {
        InstanceState((instance => instance.CurrentState));

        Event(() => OrderStartedEvent,
            configurator => configurator.CorrelateBy<int>(instance => instance.OrderId, @event => @event.Message.OrderId).SelectId(e => Guid.NewGuid()));
        Event(() => StockReservedEvent,
            orderStateInstance => orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));

        Event(() => StockNotReservedEvent,
            orderStateInstance => orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));

        Event(() => PaymentCompletedEvent,
            orderStateInstance => orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));

        Event(() => PaymentFailedEvent,
            orderStateInstance => orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));

        Initially(When(OrderStartedEvent)
            .Then(context =>
            {
                context.Instance.OrderId = context.Data.OrderId;
                context.Instance.BuyerId = context.Data.BuyerId;
                context.Instance.TotalPrice = context.Data.TotalPrice;
                context.Instance.CreatedDate = DateTime.UtcNow;
            })
            .TransitionTo(OrderCreated)
            .Send(new Uri($"queue:{RabbitMQSettings.Stock_OrderCreatedEventQueue}"),
                context => new OrderCreatedEvent(context.Instance.CorrelationId)
                {
                    OrderItems = context.Data.OrderItems.Select(x =>
                         new OrderItemMessage()
                         {
                             Quantity = x.Quantity,
                             Price = x.Price,
                             ProductId = x.ProductId
                         }
                            ).ToList()
                }));
    }
}