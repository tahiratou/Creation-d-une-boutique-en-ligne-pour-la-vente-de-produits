using Microsoft.EntityFrameworkCore;
using BoutiqueEnLigne.Data;
using BoutiqueEnLigne.Services;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Configuration de la base de données
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// Configuration des sessions
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Enregistrer les services
builder.Services.AddHttpClient<FakeStoreService>();
builder.Services.AddScoped<DbInitializerService>();
// Enregistrer FactureService
builder.Services.AddScoped<FactureService>();
// Configuration Stripe
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];



var app = builder.Build();

// Initialiser la base de données
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbInitializer = services.GetRequiredService<DbInitializerService>();
        await dbInitializer.InitializeAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Erreur lors de l'initialisation de la base de données.");
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
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();