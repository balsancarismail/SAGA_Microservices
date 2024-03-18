using MassTransit;
using Microsoft.EntityFrameworkCore;
using SagaStateMachine.StateDbContext;
using SagaStateMachine.StateInstances;
using SagaStateMachine.StateMachines;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMassTransit(configure: configurator =>
{

    configurator.AddSagaStateMachine<OrderStateMachine, OrderStateInstance>()
        .EntityFrameworkRepository(options =>
        {
            options.AddDbContext<DbContext, OrderStateDbContext>((provider, _builder) =>
            {
                _builder.UseSqlServer(builder.Configuration.GetConnectionString("MsSQL"));
            });
        });

    configurator.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ"]);
    });
});

var host = builder.Build();
host.Run();
