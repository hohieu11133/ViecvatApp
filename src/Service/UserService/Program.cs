using Microsoft.EntityFrameworkCore;
using UserService.Data;


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
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
