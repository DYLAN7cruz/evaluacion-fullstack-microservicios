using ProductsService.Application.Ports;
using ProductsService.Application.Services;
using ProductsService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Inyección de dependencias
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddInfrastructureServices(builder.Configuration);

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
