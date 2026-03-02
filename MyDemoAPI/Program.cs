using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Model.Models;
using Repository;
using Service;
using Service.Dtos.Response;
using System.Text;
using System.Text.Json.Serialization;

//var builder = WebApplicationBuilder.CreateBuilder(args);
// Đổi từ WebApplicationBuilder thành WebApplication
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Thêm cấu hình này
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "JWT Authentication API",
        Version = "v1",
        Description = "ASP.NET Core Web API with JWT Bearer Authentication"
    });

    // Cấu hình Bearer Authentication trong Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

// Add DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<BearManagementContext>(options =>
    options.UseSqlServer(connectionString));

// Add repositories
builder.Services.AddScoped<IAccountRepository, BearAccountRepository>();

// Add services
builder.Services.AddScoped<IBearAccountService, BearAccountService>();
builder.Services.AddScoped<IJwtService, JwtService>();
// Tìm dòng này:
// builder.Services.AddScoped<IOtpService, OtpService>();

// Và sửa thành:
builder.Services.AddSingleton<IOtpService, OtpService>();
// Add Email Service (cấu hình SMTP)
var emailConfig = builder.Configuration.GetSection("EmailSettings");
builder.Services.AddScoped<IEmailService>(sp => new EmailService(
    emailConfig["SmtpServer"],
    int.Parse(emailConfig["SmtpPort"]),
    emailConfig["SenderEmail"],
    emailConfig["SenderPassword"]
));

// Add Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateIssuer = false,
            ValidateAudience = false,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };
        options.Events = new JwtBearerEvents
        {
            // Tùy chỉnh lỗi 401 (Khi Token sai, hết hạn hoặc không có Token)
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                var response = new ErrorResponse(401, "Bạn chưa đăng nhập hoặc Token không hợp lệ.");
                await context.Response.WriteAsJsonAsync(response);
            },

            // Tùy chỉnh lỗi 403 (Khi đã đăng nhập nhưng Role không phải 1 hoặc 2)
            OnForbidden = async context =>
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";
                var response = new ErrorResponse(403, "Bạn không có quyền truy cập vào chức năng này.");
                await context.Response.WriteAsJsonAsync(response);
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
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
