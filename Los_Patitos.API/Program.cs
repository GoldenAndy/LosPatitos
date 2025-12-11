using Los_Patitos.Business;
using Los_Patitos.Data;
using Los_Patitos.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//Controlador del API
builder.Services.AddControllers();


//Llamadas desde el MVC con CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMvc", policy => policy.WithOrigins("https://localhost:7200").AllowAnyHeader().AllowAnyMethod());//puerto del proyecto MVC
}); 


//Conexion con bdd
var cs = builder.Configuration.GetConnectionString("MySqlConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseMySql(cs, ServerVersion.AutoDetect(cs)));

//Autenticacion con JWT
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, ValidateAudience = true, ValidateIssuerSigningKey = true, ValidateLifetime = true, ValidIssuer = jwtIssuer, ValidAudience = jwtAudience, IssuerSigningKey = signingKey, ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

//Repositorios y servicios del proyecto principal
builder.Services.AddScoped<IComercioRepository, ComercioRepository>();
builder.Services.AddScoped<IComercioService, ComercioService>();

builder.Services.AddScoped<ICajaRepository, CajaRepository>();
builder.Services.AddScoped<ICajaService, CajaService>();

builder.Services.AddScoped<ISinpeRepository, SinpeRepository>();
builder.Services.AddScoped<ISinpeService, SinpeService>();

builder.Services.AddScoped<IConfiguracionComercioRepository, ConfiguracionComercioRepository>();
builder.Services.AddScoped<IConfiguracionComercioService, ConfiguracionComercioService>();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseHttpsRedirection();

//CORS antes de Auth
app.UseCors("AllowMvc");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
