using ExpenseAuthApi.Data;
using ExpenseAuthApi.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddControllers(); // ✅ تسجيل Controllers

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddTransient<HandleExceptionMiddleware>();

var app = builder.Build();

app.UseMiddleware<HandleExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers(); // ✅ ربط Routes تبع Controllers

app.Run();