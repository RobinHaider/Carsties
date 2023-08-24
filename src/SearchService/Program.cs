using MongoDB.Driver;
using MongoDB.Entities;
using Polly.Extensions.Http;
using Polly;
using SearchService.Data;
using SearchService.Models;
using SearchService.Services;
using System.Net;
using MassTransit;
using SearchService.Consumers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// add automapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// add http client
builder.Services.AddHttpClient<AuctionServiceHttpClient>().AddPolicyHandler(GetPolicy());

// add mass transit
builder.Services.AddMassTransit(x =>
{
    // add the consumers
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();

    // name formater
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));

    // add the rabbit mq host
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

// intialize mongo db
app.Lifetime.ApplicationStarted.Register(async () =>
{
    await Policy.Handle<TimeoutException>()
        .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(10))
        .ExecuteAndCaptureAsync(async () => await DbInitializer.InitializeAsync(app));

});

app.Run();

// poly policy for http client
static IAsyncPolicy<HttpResponseMessage> GetPolicy()
    => HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
        .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));
