using ProductService.Extensions;
using ProductService.Authorization;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProductDatabase(builder.Configuration);
builder.Services.AddProductApplication();
builder.Services.AddProductLogging(builder.Configuration);
builder.Services.AddRedisCache(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOrOwner", policy =>
    {
        policy.Requirements.Add(new AdminOrOwnerRequirement());
    });
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