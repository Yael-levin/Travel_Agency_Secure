using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using TravelAgency_Secure.Data;
using TravelAgency_Secure.Filters;
using TravelAgency_Secure.Services;



var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;


// Add services to the container.
builder.Services.AddScoped<SystemMaintenanceFilter>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<SystemMaintenanceFilter>();
});
builder.Services.AddScoped<EmailService>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Trips/Trips");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseStatusCodePagesWithRedirects("/Trips/Trips?error=invalid"); //race condition 404 error handle

app.UseSession();


app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Trips}/{action=Trips}/{id?}");

app.Run();

