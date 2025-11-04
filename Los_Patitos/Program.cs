using Microsoft.EntityFrameworkCore;
using Los_Patitos.Data;
using Los_Patitos.Repositories;
using Los_Patitos.Business;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();


var cs = builder.Configuration.GetConnectionString("MySqlConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(cs, ServerVersion.AutoDetect(cs)));


builder.Services.AddScoped<IComercioRepository, ComercioRepository>();
builder.Services.AddScoped<IComercioService, ComercioService>();
builder.Services.AddScoped<ITipoIdentificacionRepository, TipoIdentificacionRepository>();
builder.Services.AddScoped<ITipoComercioRepository, TipoComercioRepository>();
builder.Services.AddScoped<ITipoIdentificacionService, TipoIdentificacionService>();
builder.Services.AddScoped<ITipoComercioService, TipoComercioService>();
builder.Services.AddScoped<ICajaService, CajaService>();
builder.Services.AddScoped<ISinpeRepository, SinpeRepository>();
builder.Services.AddScoped<ISinpeService, SinpeService>();
builder.Services.AddScoped<ICajaRepository, CajaRepository>();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
