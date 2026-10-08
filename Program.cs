using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Services;

namespace WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews(options =>
            {
                options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((value, field) => $"Значение '{value}' недопустимо для поля {field}");
                options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(field => "Поле обязательно для заполнения");

                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            });

            builder.Services.AddDbContext<WebApplication1.Models.WebstoreContext>(option =>
                option.UseSqlServer(builder.Configuration.GetConnectionString("con")));

            builder.Services.AddMemoryCache();
            builder.Services.AddScoped<CatalogCache>();

            builder.Services.Configure<FormOptions>(o => o.MultipartBodyLengthLimit = 3 * 1024 * 1024);

            builder.Services.AddDataProtection()
                .SetApplicationName("WebApplication1")
                .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "keys")));

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.Cookie.Name = "WebstoreAuth";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                    options.ExpireTimeSpan = TimeSpan.FromHours(1);
                    options.SlidingExpiration = true;
                });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/Home/StatusCode", "?code={0}");

            app.UseHttpsRedirection();

            app.Use(async (context, next) =>
            {
                var h = context.Response.Headers;
                h["X-Content-Type-Options"] = "nosniff";
                h["X-Frame-Options"] = "DENY";
                h["Referrer-Policy"] = "strict-origin-when-cross-origin";
                h["Content-Security-Policy"] =
                    "default-src 'self'; img-src 'self' data:; style-src 'self' 'unsafe-inline'; script-src 'self'; frame-ancestors 'none'";
                await next();
            });

            app.UseStaticFiles();

            var ruCulture = new CultureInfo("ru-RU");
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture(ruCulture),
                SupportedCultures = new List<CultureInfo> { ruCulture },
                SupportedUICultures = new List<CultureInfo> { ruCulture }
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
