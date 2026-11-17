using System.Text;
using CEMIS.Common;
using CEMIS.Models;
using CEMIS.Services.Implementations;
using CEMIS.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Database PostgreSQL (Neon Cloud)
builder.Services.AddDbContext<CemisDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpContextAccessor();

// 2. Đăng ký các Business Services
builder.Services.AddScoped<ICurrentUserAccess, CurrentUserAccess>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IToChucService, ToChucService>();
builder.Services.AddScoped<IHeSoPhatThaiService, HeSoPhatThaiService>();
builder.Services.AddScoped<ICalculationEngineService, CalculationEngineService>();
builder.Services.AddScoped<IDuLieuHoatDongService, DuLieuHoatDongService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IBaoCaoService, BaoCaoService>();
builder.Services.AddScoped<IPdfReportService, PdfReportService>();
builder.Services.AddScoped<INguoiDungService, NguoiDungService>();

// 3. Cấu hình JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "CemisSuperSecretKeyJwtAuth2026Iso14064StandardNetZero!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "CEMIS_Server";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "CEMIS_Client";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// 4. Cấu hình CORS cho React Frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiExceptionFilter>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// 5. Cấu hình Swagger với Bearer Token Authorization
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CEMIS - Carbon Emission Management & Information System API",
        Version = "v1",
        Description = "Hệ thống quản lý, tính toán kiểm kê phát thải Khí nhà kính (GHG) & Báo cáo ESG theo chuẩn ISO 14064-1 & GHG Protocol."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nhập token theo định dạng: Bearer {your JWT token}",
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
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CEMIS API v1");
    });
}

app.UseCors("AllowAll");

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// Tự động cập nhật Migration và gieo dữ liệu hạt giống Scope 3 mở rộng (FR-10)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CemisDbContext>();
    await CEMIS.Common.DataSeeder.ApplyMigrationsAsync(db);
    await CEMIS.Common.DataSeeder.SeedScope3Async(db);
}

app.MapControllers();

app.Run();
