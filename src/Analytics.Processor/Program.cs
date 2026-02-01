using Analytics.Processor.Consumers;
using Analytics.Processor.Data;
using Analytics.Processor.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

// Configure EF Core with PostgreSQL
builder.Services.AddDbContext<AnalyticsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AnalyticsDb")));

// Register Kafka producer service
builder.Services.AddSingleton<IKafkaProducerService, KafkaProducerService>();

// Configure MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCreatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
host.Run();
