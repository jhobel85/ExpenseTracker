using ExpenseTrackerAPI.Services;
using ExpenseTrackerAPI;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5000", "https://localhost:5001");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Expense Tracker API",
        Version = "v1",
        Description = "API for tracking expenses with date filtering and categorization"
    });
    
    // Add example for DateTime in Swagger
    options.MapType<DateTime>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "date-time",
        Example = new Microsoft.OpenApi.Any.OpenApiString(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"))
    });
});
builder.Services.AddScoped<DatabaseService>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// Initialize database
var masterConnectionString = app.Configuration.GetConnectionString("DefaultConnection")!.Replace("ExpenseTracker", "master");
await DatabaseInitializer.InitializeAsync(masterConnectionString, app.Environment.ContentRootPath);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseHttpsRedirection();
app.MapControllers();
    
app.Run();
