using Dental_Clinic_System.Areas.Admin.Models;
using Dental_Clinic_System.Helper;
using Dental_Clinic_System.Models.Data;
using Dental_Clinic_System.Services.EmailSender;
using Dental_Clinic_System.Services.EmailVerification;
using Dental_Clinic_System.Services.GoogleSecurity;
using Dental_Clinic_System.Services.MOMO;
using Dental_Clinic_System.Services.VNPAY;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Reflection;
using System.Runtime.Loader;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<DentalClinicDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("DBConnection")));
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(30);  // Set session timeout to 30 minutes
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

}).AddCookie("DentistScheme", options =>
{
	options.LoginPath = "/Dentist/DentistAccount/Login";
	options.AccessDeniedPath = "/Dentist/DentistAccount/AccessDenied";
	options.ExpireTimeSpan = TimeSpan.FromDays(14);

}).AddCookie("GetAppointmentStatus", options =>
{
    options.LoginPath = "/Admin/AdminAccount/Login";
    options.AccessDeniedPath = "/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
}).AddCookie("ManagerScheme", options =>
{
	options.LoginPath = "/Manager/ManagerAccount/Login";
	options.AccessDeniedPath = "/Manager/ManagerAccount/AccessDenied";
	options.ExpireTimeSpan = TimeSpan.FromDays(14);
}).AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
	options.ClientId = builder.Configuration.GetSection("GoogleKeys:ClientId").Value;
	options.ClientSecret = builder.Configuration.GetSection("GoogleKeys:ClientSecret").Value;
    options.SaveTokens = true;
    options.Events.OnCreatingTicket = async context =>
    {
        var tokens = context.Properties.GetTokens().ToList();
        tokens.Add(new AuthenticationToken()
        {
            Name = "TicketCreated",
            Value = DateTime.UtcNow.ToString()
        });
        context.Properties.StoreTokens(tokens);
    };
});

// Register Google Security Service API
builder.Services.AddScoped<GoogleSecurity>();

// Register the background service
builder.Services.AddHostedService<AppointmentReminder>();

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

// Register MOMO Service for checkout
builder.Services.AddScoped<IMOMOPayment, MOMOPayment>();

// Configure HttpClient
builder.Services.AddHttpClient();

// Register EmailVerification (ZeroBounce) Service for Verificating Email  
builder.Services.AddSingleton<IEmailVerification, EmailVerification>();



// Register Redis Caching Service
//builder.Services.AddStackExchangeRedisCache(options =>
//{
//	options.Configuration = builder.Configuration.GetSection("RedisConnection").GetValue<string>("Configuration");
//	options.InstanceName = builder.Configuration.GetSection("RedisConnection").GetValue<string>("InstanceName");

//});

//Soyu: Cấu hình DinkToPDF
var context = new CustomAssemblyLoadContext();
var wkHtmlToPdfPath = Path.Combine(Directory.GetCurrentDirectory(), "wkhtmltox", "libwkhtmltox.dll");
context.LoadUnmanagedLibrary(wkHtmlToPdfPath);

//Soyu: Thêm Singleton cho phần xuất pdf
builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

//Soyu: Đăng Ký PDFService
builder.Services.AddSingleton<PdfService>();

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

app.UseCors(builder =>
{
    builder.WithOrigins("https://your-allowed-origin.com")
           .AllowAnyHeader()
           .AllowAnyMethod();
});

//============ ADMIN ================
app.UseEndpoints(endpoints =>
{
    _ = endpoints.MapControllerRoute(
       name: "admin_default",
       pattern: "Admin",
       defaults: new { area = "Admin", controller = "Dashboard", action = "GetAppointmentStatus" });

    _ = endpoints.MapControllerRoute(
      name: "areas",
      pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
    );
});


//============ Dentist Route ================
app.UseEndpoints(endpoints =>
{
	_ = endpoints.MapControllerRoute(
	   name: "dentist_default",
	   pattern: "Dentist",
	   defaults: new { area = "Dentist", controller = "DentistDetail", action = "DentistSchedule" });

	_ = endpoints.MapControllerRoute(
			name: "areas",
			pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
});
//============ Manager Route ================
app.UseEndpoints(endpoints =>
{
	_ = endpoints.MapControllerRoute(
	   name: "manager_default",
	   pattern: "Manager",
	   defaults: new { area = "Manager", controller = "Dentists", action = "Index" });

	_ = endpoints.MapControllerRoute(
			name: "areas",
			pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
});
//===========================================

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

#region Class CustomAssemblyLoadContext
public class CustomAssemblyLoadContext : AssemblyLoadContext
{
    public IntPtr LoadUnmanagedLibrary(string absolutePath)
    {
        return LoadUnmanagedDll(absolutePath);
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        return LoadUnmanagedDllFromPath(unmanagedDllName);
    }

    protected override Assembly Load(AssemblyName assemblyName)
    {
        throw new NotImplementedException();
    }
}
#endregion

#region Class PdfService
public class PdfService
{
    private readonly IConverter _converter;

    public PdfService(IConverter converter)
    {
        _converter = converter;
    }

    public byte[] GeneratePdf(string html)
    {
        var doc = new HtmlToPdfDocument()
        {
            GlobalSettings = {
                ColorMode = DinkToPdf.ColorMode.Color,
                Orientation = DinkToPdf.Orientation.Portrait,
                PaperSize = DinkToPdf.PaperKind.A4
            },
            Objects = {
                new ObjectSettings() {
                    PagesCount = null,
                    HtmlContent = html,
                    WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/assets/css", "appointmentpdf.css") },
                }
            }
        };

        return _converter.Convert(doc);
    }
}
#endregion
