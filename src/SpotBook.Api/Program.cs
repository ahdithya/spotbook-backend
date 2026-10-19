using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using SpotBook.Application.Common.Interfaces;
using SpotBook.Application.Interfaces;
using SpotBook.Application.Interfaces.Auth;
using SpotBook.Application.Interfaces.Spaces;
using SpotBook.Application.Interfaces.Vouchers;
using SpotBook.Application.Services.Auth;
using SpotBook.Application.Services.Spaces;
using SpotBook.Application.Services.Vouchers;
using SpotBook.Infrastructure.Persistance;
using SpotBook.Infrastructure.services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));


builder.Services.AddScoped<IAppDbContext>(provider =>
    provider.GetRequiredService<AppDbContext>());

// services registrations
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISpaceService, SpaceService>();
builder.Services.AddScoped<IVoucherService, VoucherService>();


// JWT Authentications
var jwtSecret = builder.Configuration["JwtSettings:Secret"]
    ?? throw new InvalidOperationException("JWT Secret belum dikonfigurasi.");
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"];
var jwtAudience = builder.Configuration["JwtSettings:Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero // Menghilangkan toleransi waktu default 5 menit agar expired tepat waktu
    };
});


builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
    c.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });
    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});
builder.Services.AddOpenApi();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    // 2. Tampilkan UI Swagger dan arahkan ke dokumen Swagger yang benar
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SpotBook API v1");
        options.RoutePrefix = "swagger"; // Bisa diakses di http://localhost:5135/swagger
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
