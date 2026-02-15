using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SewaRuangan.API.Data;
using SewaRuangan.API.Data.Seed;
using SewaRuangan.API.Helpers;

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

// 2. TAMBAHKAN CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                   .AllowAnyHeader()
                   .AllowAnyMethod();
        });
});

// Add JWT Configuration
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

// Add JwtHelper as Singleton
builder.Services.AddSingleton<JwtHelper>();

// Add Controllers
builder.Services.AddControllers();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add IWebHostEnvironment
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

app.UseCors("AllowAll"); // CORS harus sebelum Auth

app.UseAuthentication(); // Tambahkan ini!

app.UseAuthorization();

app.MapControllers();

app.Run();