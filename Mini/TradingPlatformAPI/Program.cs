using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using TradingPlatform.Config;
using TradingPlatform.Repositories;
using TradingPlatform.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Configuration ──────────────────────────────────────────────────────────
var mongoSettings = builder.Configuration
    .GetSection("MongoDbSettings")
    .Get<MongoDbSettings>()!;

var jwtSettings = builder.Configuration
    .GetSection("JwtSettings")
    .Get<JwtSettings>()!;

// ── MongoDB ────────────────────────────────────────────────────────────────
builder.Services.AddSingleton<IMongoClient>(_ =>
    new MongoClient(mongoSettings.ConnectionString));

builder.Services.AddSingleton<IMongoDatabase>(sp =>
    sp.GetRequiredService<IMongoClient>().GetDatabase(mongoSettings.DatabaseName));

// ── Repositories ───────────────────────────────────────────────────────────
builder.Services.AddSingleton<UserRepository>();
builder.Services.AddSingleton<InstrumentRepository>();
builder.Services.AddSingleton<OrderRepository>();
builder.Services.AddSingleton<TradeRepository>();
builder.Services.AddSingleton<FixMessageRepository>();

// ── Services ───────────────────────────────────────────────────────────────
builder.Services.AddSingleton(jwtSettings);
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<BillingService>();
builder.Services.AddSingleton<FixMessageService>();
builder.Services.AddSingleton<MatchingEngineService>();
builder.Services.AddSingleton<OrderService>();

// ── Authentication (JWT Bearer) ────────────────────────────────────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
        };
    });

// ── ASP.NET Core ────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ── Middleware ─────────────────────────────────────────────────────────────
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
