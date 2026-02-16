using Microsoft.EntityFrameworkCore;
using PastisserieAPI.Infrastructure.Data;
using PastisserieAPI.Core.Interfaces;
using PastisserieAPI.Core.Interfaces.Repositories;
using PastisserieAPI.Infrastructure.Repositories;
using FluentValidation;
using System.Reflection;
using PastisserieAPI.Services.Helpers;
using PastisserieAPI.Services.Services.Interfaces;
using PastisserieAPI.Services.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ============ CONFIGURACIÓN DE SERVICIOS ============

// Add services to the container.
builder.Services.AddControllers();

// Configurar DbContext con SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null
        )
    )
);

// Inyección de dependencias - Unit of Work Pattern
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Inyección de dependencias - Repositorios (opcional, si quieres usarlos directamente)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
builder.Services.AddScoped<ICarritoRepository, CarritoRepository>();
builder.Services.AddScoped<IEnvioRepository, EnvioRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();

// ========== BACKGROUND SERVICE PARA LIBERAR RESERVAS (RN2) ==========
builder.Services.AddHostedService<ReservaStockService>();

// AutoMapper - Cargar desde el ensamblado de Services donde está MappingProfile
builder.Services.AddAutoMapper(typeof(PastisserieAPI.Services.Mappings.MappingProfile));

// FluentValidation
builder.Services.AddValidatorsFromAssembly(Assembly.Load("PastisserieAPI.Services"));

// JWT Helper
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddSingleton(new JwtHelper(
    jwtSettings["SecretKey"]!,
    jwtSettings["Issuer"]!,
    jwtSettings["Audience"]!,
    int.Parse(jwtSettings["ExpirationMinutes"]!)
));

// Servicios de negocio
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddScoped<ICarritoService, CarritoService>();
builder.Services.AddScoped<IReviewService, ReviewService>();

// Autenticación JWT
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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!)
        ),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Swagger - COMENTADO TEMPORALMENTE
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

// Swagger con JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Pastisserie API",
        Version = "v1",
        Description = "API para gestión de panadería y pastelería"
    });

    // Configuración de seguridad JWT
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "Ingrese el token JWT en el formato: Bearer {token}",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ============ CONFIGURACIÓN DEL PIPELINE HTTP ============

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pastisserie API V1");
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ============ MENSAJE DE INICIO ============
app.Logger.LogInformation("🍰 Pastisserie API iniciada correctamente");
app.Logger.LogInformation("📊 Base de datos: {ConnectionString}",
    builder.Configuration.GetConnectionString("DefaultConnection")?.Split(';')[0]);

app.Run();