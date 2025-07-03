using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Dialect;
using NHibernate.Tool.hbm2ddl;
using Proyect_InvOperativa.Mapping;
using Proyect_InvOperativa.Models;
using Proyect_InvOperativa.Repository;
using Proyect_InvOperativa.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// BASE DE DATOS
var connectionString = builder.Configuration.GetConnectionString("MySQLConnection");
builder.Services.AddSingleton<ISessionFactory>(provider =>
{
    Console.WriteLine("Conectando a bd");
    return Fluently.Configure()
        .Database(
            
                MySQLConfiguration.Standard
                .ConnectionString(connectionString)
                .Dialect<MySQL8Dialect>()
                .ShowSql()
        )
        .Mappings(m => m.FluentMappings.AddFromAssemblyOf<ArticuloMapping>())
        .ExposeConfiguration(cfg => new SchemaUpdate(cfg).Execute(false, true))
        .BuildSessionFactory();
});

builder.Services.AddScoped<NHibernate.ISession>(provider =>
    provider.GetRequiredService<ISessionFactory>().OpenSession());

// Registro de repositorios
builder.Services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
builder.Services.AddScoped<ArticuloRepository>();
builder.Services.AddScoped<EstadoProveedoresRepository>();
builder.Services.AddScoped<MaestroArticulosRepository>();
builder.Services.AddScoped<ProveedoresRepository>();
builder.Services.AddScoped<ProveedorArticuloRepository>();
builder.Services.AddScoped<OrdenCompraEstadoRepository>();
builder.Services.AddScoped<ProveedoresRepository>();
builder.Services.AddScoped<ProveedorEstadoRepository>();
builder.Services.AddScoped<VentasRepository>();
builder.Services.AddScoped<OrdenCompraRepository>();
builder.Services.AddScoped<StockArticuloRepository>();
builder.Services.AddScoped<DetalleOrdenCompraRepository>();
builder.Services.AddScoped<BaseRepository<DetalleVentas>>();

//Registro de Servicios
builder.Services.AddScoped<MaestroArticulosService>();
builder.Services.AddScoped<OrdenCompraService>();
builder.Services.AddScoped<VentasService>();
builder.Services.AddScoped<ProveedorArticuloService>();
builder.Services.AddScoped<OrdenCompraEstadoService>();
builder.Services.AddScoped<DetalleOrdenCompraService>();
builder.Services.AddScoped<ProveedorService>();
builder.Services.AddScoped<ProveedorEstadoService>();
builder.Services.AddHostedService<ControlStockPeriodoFijoService>();

var apiBaseRoute = builder.Configuration.GetValue<string>("ApiBaseRoute");

builder.Services.AddControllers(); 

// raro pero parece funcionar
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:8008")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("Access-Control-Allow-Private-Network")
              .SetIsOriginAllowedToAllowWildcardSubdomains();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
    {
    try
        {
        var session = scope.ServiceProvider.GetRequiredService<NHibernate.ISession>();
        var result = session.CreateSQLQuery("SELECT 1").UniqueResult();
        Console.WriteLine("✅ Conexión a MySQL exitosa");
        }
    catch (Exception ex)
        {   
        Console.WriteLine("❌ Error de conexión: " + ex.Message);
        Exception inner = ex.InnerException!;
        while (inner != null)
            {
            Console.WriteLine("Inner Exception: " + inner.Message);
            inner = inner.InnerException!;
            }
        }
    }

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();

        }
		
app.Use(async (context, next) =>
{
    if (context.Request.Headers.ContainsKey("Access-Control-Request-Private-Network"))
    {
        context.Response.Headers.Add("Access-Control-Allow-Private-Network", "true");
    }
    await next.Invoke();
});		

app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

app.UseAuthorization();

app.Use(async (context, next) =>
{
    Console.WriteLine("Origin: " + context.Request.Headers["Origin"]);
    await next.Invoke();
});

app.MapControllers();

app.Run();