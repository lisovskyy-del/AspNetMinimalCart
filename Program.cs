using AspNetMinimalCart.Endpoints;
using AspNetMinimalCart.Middleware;
using AspNetMinimalCart.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File("logs/cart-api-.log", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web host");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File("logs/cart-api-.log", rollingInterval: RollingInterval.Day));

    builder.Services.AddDistributedMemoryCache();

    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromHours(2);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

    builder.Services.AddSingleton<ProductCatalog>();
    builder.Services.AddScoped<CartStorage>();

    var app = builder.Build();

    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseSession();

    app.Use(async (context, next) =>
    {
    var rawCookie = context.Request.Cookies["UserId"];
    var maskedUserId = string.IsNullOrEmpty(rawCookie)
        ? "Anonymous"
        : (rawCookie.Length > 8 ? $"{rawCookie[..8]}***" : "InvalidCookie");

    var endpoint = $"{context.Request.Method} {context.Request.Path}";

    using (Serilog.Context.LogContext.PushProperty("UserId", maskedUserId))
    using (Serilog.Context.LogContext.PushProperty("Endpoint", endpoint))
    {
        await next(context);
    }
    });

    app.MapGet("/", (ILogger<Program> logger) => 
    {
        logger.LogInformation("Hello, World!");
        return "Serilog is working!";
    });

    app.MapIdentifyEndpoints();
    app.MapCartItemsEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}