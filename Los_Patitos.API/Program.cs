using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using Los_Patitos.Data;
using Los_Patitos.Repositories;
using Los_Patitos.Business;

var builder = WebApplication.CreateBuilder(args);

// -------- Controllers (solo API) --------
builder.Services.AddControllers();

// -------- Swagger con soporte para Bearer --------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Los Patitos API",
        Version = "v1",
        Description = "API para sincronización SINPE externa"
    });

    // Evita colisiones de nombres de DTOs (causa clásica de 500 en /swagger/v1/swagger.json)
    c.CustomSchemaIds(t => t.FullName);

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Autenticación JWT. Ejemplo: Bearer {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

// -------- CORS (permite tu MVC en 7200) --------
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", p =>
        p.WithOrigins("https://localhost:7200", "http://localhost:7200")
         .AllowAnyHeader()
         .AllowAnyMethod());
});

// -------- DB --------
var cs = builder.Configuration.GetConnectionString("MySqlConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(cs, ServerVersion.AutoDetect(cs)));

// -------- JWT (mismos valores que tu MVC) --------
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// -------- DI (repos & services que reutilizas) --------
builder.Services.AddScoped<IComercioRepository, ComercioRepository>();
builder.Services.AddScoped<IComercioService, ComercioService>();
builder.Services.AddScoped<ITipoIdentificacionRepository, TipoIdentificacionRepository>();
builder.Services.AddScoped<ITipoComercioRepository, TipoComercioRepository>();
builder.Services.AddScoped<ITipoIdentificacionService, TipoIdentificacionService>();
builder.Services.AddScoped<ITipoComercioService, TipoComercioService>();
builder.Services.AddScoped<ICajaRepository, CajaRepository>();
builder.Services.AddScoped<ICajaService, CajaService>();
builder.Services.AddScoped<ISinpeRepository, SinpeRepository>();
builder.Services.AddScoped<ISinpeService, SinpeService>();
builder.Services.AddScoped<IBitacoraService, BitacoraService>();
builder.Services.AddScoped<IConfiguracionComercioRepository, ConfiguracionComercioRepository>();
builder.Services.AddScoped<IConfiguracionComercioService, ConfiguracionComercioService>();
builder.Services.AddScoped<IReporteMensualRepository, ReporteMensualRepository>();
builder.Services.AddScoped<IReporteMensualService, ReporteMensualService>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

var app = builder.Build();

// -------- Pipeline --------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
