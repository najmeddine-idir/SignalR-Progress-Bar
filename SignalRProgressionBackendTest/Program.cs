using Microsoft.AspNetCore.SignalR;
using Microsoft.Net.Http.Headers;
using SignalRProgressionBackendTest;
using SignalRProgressionBackendTest.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddCors(options => options.AddPolicy(name: "test",
    policy =>
    {
        policy.WithOrigins(["http://localhost:5234"])
              .AllowAnyHeader()
              .SetIsOriginAllowed(origin => true)
              .AllowCredentials()
              .AllowAnyMethod();
    }));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseCors("test");

app.MapHub<ProgressHub>("/progressHub");

app.MapPost("/progress", async (IHubContext<ProgressHub> hubContext) =>
{
    var numberList = Enumerable.Range(1, 500);
    var startTime = DateTime.Now;

    await numberList.ParallelForEachAsync(async x =>
    {
        Thread.Sleep(Random.Shared.Next(60, 280));

        var timeRemaining = DateTime.Now.Subtract(startTime).Ticks * (500 - (x + 1)) / (x + 1);

        await hubContext.Clients.All.SendAsync(
            "ReceiveProgress", 
            "userTest", 
            new { 
                progressPercentage = $"{Math.Round(decimal.Divide(x, 500) * 100, 1)}%".Replace(",", "."), 
                remainingTime = $"{TimeSpan.FromTicks(timeRemaining > 0 ? timeRemaining : 0):hh\\:mm\\:ss}"
            });

    }, maxDegreeOfParallelism: 100);
})
.WithName("PostProgress")
.WithOpenApi();

app.Run();
