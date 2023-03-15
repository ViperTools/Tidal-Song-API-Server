var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins("*");
        policy.WithHeaders("Content-Type", "Authorization");
    });
});

var app = builder.Build();

// WebSockets

app.UseWebSockets();

app.Use(async (context, next) =>
{
    if (context.Request.Path != "/ws")
    {
        await next(context);
        return;
    }

    if (context.WebSockets.IsWebSocketRequest)
    {
        Guid id = WebSocketManager.AddWebSocket(await context.WebSockets.AcceptWebSocketAsync());
        await WebSocketManager.HandleMessages(id);
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors("CorsPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();
