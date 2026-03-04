using FinLedger.Api.Endpoints;
using FinLedger.Api.Middleware;
using FinLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FinLedger API",
        Version = "v1",
        Description = "Double-entry ledger service for financial transactions. Portfolio project aligned with Senior .NET Backend (fintech/POS) roles."
    });
    options.OperationFilter<FinLedger.Api.Swagger.SwaggerExampleFilter>();
});

builder.Services.AddDbContext<LedgerDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.UseMiddleware<ExceptionHandlerMiddleware>();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LedgerDbContext>();
    await db.Database.EnsureCreatedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "FinLedger API v1"));
}

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .WithName("HealthCheck")
    .WithOpenApi();

var apiV1 = app.MapGroup("/api/v1").WithTags("API v1");
apiV1.MapAccountEndpoints();
apiV1.MapTransactionEndpoints();

app.Run();
