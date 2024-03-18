using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderAPI.DbContext;
using OrderAPI.Models;
using OrderAPI.ViewModel;
using Shared;
using Shared.OrderEvents;

var builder = WebApplication.CreateBuilder(args);
    
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(configure: configurator =>
{
    configurator.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ"]);
    });
});

builder.Services.AddDbContext<OrderDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("MsSQL"));
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


app.MapPost("/create-order", async (CreateOrderModel model, OrderDbContext context, ISendEndpointProvider sendEndpointProvider) =>
{
    Order order = new()
    {
        BuyerId = model.BuyerId,
        CreatedDate = DateTime.UtcNow,
        OrderStatus = OrderStatus.Suspend,
        TotalPrice = model.OrderItemModels.Sum(oi => oi.Quantity * oi.Price),
        OrderItems = model.OrderItemModels.Select(oi => new OrderItem
        {
            Price = oi.Price,
            Quantity = oi.Quantity,
            ProductId = oi.ProductId,
        }).ToList(),
    };

    await context.Orders.AddAsync(order);
    await context.SaveChangesAsync();

    OrderStartedEvent orderStartedEvent = new()
    {
        BuyerId = model.BuyerId,
        OrderId = order.Id,
        TotalPrice = model.OrderItemModels.Sum(oi => oi.Quantity * oi.Price),
        OrderItems = model.OrderItemModels.Select(oi => new Shared.Messages.OrderItemMessage
        {
            Price = oi.Price,
            Quantity = oi.Quantity,
            ProductId = oi.ProductId
        }).ToList()
    };

    var sendEndpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.StateMachineQueue}"));
    await sendEndpoint.Send<OrderStartedEvent>(orderStartedEvent);

});

app.Run();
