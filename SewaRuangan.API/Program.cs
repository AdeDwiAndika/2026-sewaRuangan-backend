using Microsoft.EntityFrameworkCore;
using SewaRuangan.API.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 1. Configure Database dengan PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
        npgsqlOptions.EnableRetryOnFailure(5);
    }));

// 2. Add Controllers
builder.Services.AddControllers();

// 3. Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 4. Add IWebHostEnvironment
builder.Services.AddSingleton<IWebHostEnvironment>(builder.Environment);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
    // ✅ HANYA migrate, tidak create database (karena database sudah ada)
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        // Cek apakah database bisa connect
        if (dbContext.Database.CanConnect())
        {
            // Apply migrations jika ada
            dbContext.Database.Migrate();
            Console.WriteLine("✅ Database migration completed.");
        }
        else
        {
            Console.WriteLine("❌ Cannot connect to database. Please check connection string.");
        }
    }
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();