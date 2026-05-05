using ProductService.Data;
using Microsoft.EntityFrameworkCore;
using ProductService.Application.Handlers;
using ProductService.Application.Interfaces;
using ProductService.Infrastructure.Repositories;
using ProductService.Application.Events;
using ProductService.Infrastructure.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddHttpClient();
var dbPath = Path.Combine(AppContext.BaseDirectory, "products.db");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<CreateProductHandler>();
builder.Services.AddScoped<UpdateProductHandler>();
builder.Services.AddScoped<IEventPublisher, HttpEventPublisher>();
builder.Services.AddScoped<GetAllProductsHandler>();
builder.Services.AddScoped<GetProductByIdHandler>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();
app.Run();

