using TransactionsService.Application.Ports;
using TransactionsService.Application.Services;
using TransactionsService.Infrastructure;
using TransactionsService.Infrastructure.HttpClients;

var builder = WebApplication.CreateBuilder(args);

// Inyección de dependencias
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Cliente HTTP síncrono para comunicarse con ProductsService
var productsServiceUrl = Environment.GetEnvironmentVariable("PRODUCTS_SERVICE_URL")
    ?? builder.Configuration["ProductsService:Url"]
    ?? "http://localhost:5001";

builder.Services.AddHttpClient<IProductServiceClient, ProductServiceClient>(client =>
{
    client.BaseAddress = new Uri(productsServiceUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

// Controladores y OpenAPI
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Configuración CORS para Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAngular");
app.UseAuthorization();
app.MapControllers();

app.Run();
