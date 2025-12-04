using Microsoft.EntityFrameworkCore;
using Los_Patitos.Data;
using Los_Patitos.Repositories;
using Los_Patitos.Business;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Los_Patitos.Models;
using OfficeOpenXml;

ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews(options =>
{
    //todo usuario tiene que estar autenticado para acceder a los views
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    options.Filters.Add(new AuthorizeFilter(policy));
});

var cs = builder.Configuration.GetConnectionString("MySqlConnection");

builder.Services.AddDbContext<AppDbContext>(options => options.UseMySql(cs, ServerVersion.AutoDetect(cs)));



builder.Services.AddIdentity<UsuarioIdentity, RolIdentity>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;

    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Cuenta/IniciarSesion";      //Ruta del login
    options.AccessDeniedPath = "/Cuenta/AccesoDenegado"; //Ruta cuando no tiene rol
});

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
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IConfiguracionComercioService, ConfiguracionComercioService>();
builder.Services.AddScoped<IConfiguracionComercioRepository, ConfiguracionComercioRepository>();
builder.Services.AddScoped<IReporteMensualRepository, ReporteMensualRepository>();
builder.Services.AddScoped<IReporteMensualService, ReporteMensualService>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();



var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<RolIdentity>>();

    string[] roles = new[] { "Administrador", "Cajero" };

    foreach (var roleName in roles)
    {
        var exists = await roleManager.RoleExistsAsync(roleName);
        if (!exists)
        {
            await roleManager.CreateAsync(new RolIdentity { Name = roleName });
        }
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();