using Microsoft.EntityFrameworkCore;
using UserService.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Security.Claims;
var builder = WebApplication.CreateBuilder(args);
// 1. Lấy chuỗi kết nối từ biến môi trường (Docker) hoặc appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// 2. Đăng ký DbContext với NetTopologySuite để dùng tọa độ GPS
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
        sqlOptions.UseNetTopologySuite()));
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddSingleton<UserService.Messaging.KafkaProducerService>();
builder.Services.AddEndpointsApiExplorer();
// --- CẤU HÌNH JWT AUTHENTICATION ---
var jwtSecret = builder.Configuration["JwtSettings:SecretKey"] ?? "DayLaChuoiBiMatSieuDaiVaPhucTapChoViecVatApp2026!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "ViecVatApp",
            ValidAudience = "ViecVatUsers",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            RoleClaimType = ClaimTypes.Role
        };
    });
builder.Services.AddAuthorization();

// --- CẤU HÌNH SWAGGER ĐỂ NHẬP TOKEN ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nhập 'Bearer [khoảng trắng] {token của bạn}' vào đây.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication(); //kiểm tra thẻ

app.UseAuthorization(); //kiểm tra quyền hạn

app.MapControllers();

app.Run();
