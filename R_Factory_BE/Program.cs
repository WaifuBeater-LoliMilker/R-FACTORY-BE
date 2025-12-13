using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using R_Factory_BE.Auth;
using R_Factory_BE.Middlewares;
using R_Factory_BE.Models.Context;
using R_Factory_BE.Repositories;
using R_Factory_BE.Services;
using System.Data;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<r_factoryContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

builder.Services.AddTransient<IDbConnection>(sp =>
    new MySql.Data.MySqlClient.MySqlConnection(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (string.IsNullOrWhiteSpace(jwtSettings?.Secret)) throw new InvalidOperationException("Bí mật của JWT đâu??");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("ManagersOnly", policy => policy.RequireRole("Managers"));
//.AddPolicy("MustBeRegistered", policy => policy.RequireClaim("UserType", "Registered"));
builder.Services.AddMvc().AddJsonOptions(opt => opt.JsonSerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGenericRepo, GenericRepo>();
// Add services to the container.

builder.Services.AddControllers();
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNameCaseInsensitive = false;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API lỏ", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập JWT token vào đây. Ví dụ: Bearer {token}"
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
builder.Services.AddCors();

var app = builder.Build();

// Configure the HTTP request pipeline.
var filesPath = Path.Combine(builder.Environment.ContentRootPath, "Files");
if (!Directory.Exists(filesPath))
{
    Directory.CreateDirectory(filesPath);
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "Files")),
    RequestPath = "/files"
});
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseDeveloperExceptionPage();
app.UseRouting();
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>();
app.UseCors(builder => builder.WithOrigins(allowedOrigins!)
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());

app.UseMiddleware<JwtMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();