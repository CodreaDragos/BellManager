using BellManager.Api.Data;
using BellManager.Api.Controllers;
using BellManager.Api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// Configure DbContext with Railway DATABASE_URL
var databaseUrl = builder.Configuration["ConnectionStrings:PostgresUrl"];
if (string.IsNullOrWhiteSpace(databaseUrl))
{
	throw new InvalidOperationException("Missing ConnectionStrings:PostgresUrl user-secret.");
}

// Convert DATABASE_URL (postgresql://user:pass@host:port/db) to Npgsql connection string
string BuildNpgsqlConnectionString(string url)
{
	var uri = new Uri(url);
	var userInfo = uri.UserInfo.Split(':', 2);
	var username = Uri.UnescapeDataString(userInfo[0]);
	var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
	var database = uri.AbsolutePath.Trim('/');

	var csb = new NpgsqlConnectionStringBuilder
	{
		Host = uri.Host,
		Port = uri.Port,
		Username = username,
		Password = password,
		Database = database,
		SslMode = SslMode.Require,
		TrustServerCertificate = true
	};
	return csb.ConnectionString;
}

var npgsqlConnectionString = BuildNpgsqlConnectionString(databaseUrl);

builder.Services.AddDbContext<AppDbContext>(options =>
{
	options.UseNpgsql(npgsqlConnectionString);
});

builder.Services.AddControllers();

// Add CORS to allow requests from mobile devices
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowMobile", policy =>
	{
		policy.AllowAnyOrigin()
			  .AllowAnyMethod()
			  .AllowAnyHeader();
	});
});

// JWT settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
if (jwtSettings is null || string.IsNullOrWhiteSpace(jwtSettings.Secret))
{
	throw new InvalidOperationException("Missing Jwt settings in configuration.");
}

var key = Encoding.UTF8.GetBytes(jwtSettings.Secret);
builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = jwtSettings.Issuer,
		ValidAudience = jwtSettings.Audience,
		IssuerSigningKey = new SymmetricSecurityKey(key)
	};
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Check for command line arguments
if (args.Length > 0 && args[0] == "clear-alarms")
{
	Console.WriteLine("Clearing all alarms from database...");
	using (var scope = app.Services.CreateScope())
	{
		var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
		var alarms = await db.Alarms.ToListAsync();
		Console.WriteLine($"Found {alarms.Count} alarms to delete");
		
		if (alarms.Count > 0)
		{
			db.Alarms.RemoveRange(alarms);
			await db.SaveChangesAsync();
			Console.WriteLine($"Successfully deleted {alarms.Count} alarms");
		}
		else
		{
			Console.WriteLine("No alarms found to delete");
		}
		
		var remaining = await db.Alarms.CountAsync();
		Console.WriteLine($"Remaining alarms: {remaining}");
	}
	return;
}

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseCors("AllowMobile");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed admin user at startup
using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
	var admin = await db.Users.FirstOrDefaultAsync(u => u.UserName == "Admin");
	if (admin is null)
	{
		var hashed = BCrypt.Net.BCrypt.HashPassword("admin1234");
		db.Users.Add(new User
		{
			Email = "admin@gmail.com",
			UserName = "Admin",
			PasswordHash = hashed,
			Role = "admin"
		});
		await db.SaveChangesAsync();
	}
}

// Configure the server to listen on the port provided by Railway
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");
