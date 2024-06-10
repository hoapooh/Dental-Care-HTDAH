using Dental_Clinic_System.Areas.Admin.Models;
using Dental_Clinic_System.Helper;
using Dental_Clinic_System.Models.Data;
using Dental_Clinic_System.Services;
using Dental_Clinic_System.Services.VNPAY;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<DentalClinicDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("DBConnection")));
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
	//options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(14); // Set the expiration time for persistent cookies
}).AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
    options.ClientId = builder.Configuration.GetSection("GoogleKeys:ClientId").Value;
    options.ClientSecret = builder.Configuration.GetSection("GoogleKeys:ClientSecret").Value;
});

// Register the email sender service
builder.Services.AddScoped<IEmailSenderCustom, EmailSender>();

//// Register the email sender service
//builder.Services.AddScoped<IEmailSender, EmailSender>();

// Register HiddenSpecialtyService as a singleton
builder.Services.AddSingleton<HiddenSpecialtyService>();

// Register VNPAY Service for checkout
builder.Services.AddSingleton<IVNPayment, VNPayment>();

// Register VNPAY Service for checkout with HttpClient
//builder.Services.AddHttpClient<IVNPayment, VNPayment>();

// Configure HttpClient
builder.Services.AddHttpClient();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
	endpoints.MapControllerRoute(
	  name: "areas",
	  pattern: "{area:exists}/{controller=Admin}/{action=ListAccount}/{id?}"
	);
});


//============ Dentist Route ================
app.UseEndpoints(endpoints =>
{
	_ = endpoints.MapControllerRoute(
	   name: "dentist_default",
	   pattern: "Dentist",
	   defaults: new { area = "Dentist", controller = "Dentist", action = "DentistSchedule" });

	_ = endpoints.MapControllerRoute(
			name: "areas",
			pattern: "{area:exists}/{controller=Home}/{id?}");
});
//===========================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
