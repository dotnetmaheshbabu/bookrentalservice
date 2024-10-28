using BookRentalService.DbContextHelper;
using BookRentalService.Models;
using BookRentalService.Repository;
using BookRentalService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using BookRentalService.BackgroundJob;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Get the connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Add DbContext service with SQL Server and connection string
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));


// Register repositories for dependency injection
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IRepository<Rental>, Repository<Rental>>();
builder.Services.AddScoped<IRepository<WaitingList>, Repository<WaitingList>>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IRentalRepository, RentalRepository>();
// Register the background service for sending overdue emails as a singleton
builder.Services.AddSingleton<IHostedService, EmailBackgroundService>();

// Configure SendGrid options using appsettings.json section
builder.Services.Configure<SendGridOptions>(builder.Configuration.GetSection("SendGrid"));

// Add controllers to the service container
builder.Services.AddControllers();

// Configure CORS to allow requests from all origins (you can modify this for specific origins)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Configure logging (Application Insights or other logging mechanism)
builder.Logging.AddConsole();  

// Configure Swagger/OpenAPI for API documentation and testing
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable Swagger UI for API documentation in development mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use CORS middleware to allow cross-origin requests
app.UseCors("AllowAll");

// Redirect all HTTP traffic to HTTPS
app.UseHttpsRedirection();

// Use authorization middleware (if authentication is needed)
app.UseAuthorization();

// Map controllers to handle HTTP routes
app.MapControllers();

app.Run();
