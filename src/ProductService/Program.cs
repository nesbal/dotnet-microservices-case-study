using ProductService.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProductDatabase(builder.Configuration);
builder.Services.AddProductApplication();
builder.Services.AddProductLogging();
builder.Services.AddRedisCache(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOrOwner", policy =>
        policy.RequireAssertion(context =>
        {
            var user = context.User;

            if (user.IsInRole("Admin"))
            {
                return true;
            }

            var httpContext = context.Resource as HttpContext;
            var routeId = httpContext?.Request.RouteValues["id"]?.ToString();

            if (routeId == null)
            {
                return false;
            }

            var db = httpContext.RequestServices.GetRequiredService<ProductService.Data.AppDbContext>();
            var product = db.Products.Find(int.Parse(routeId));

            return product?.OwnerUsername == user.Identity?.Name;
        }));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

var app = builder.Build();

app.ApplyMigrations();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();