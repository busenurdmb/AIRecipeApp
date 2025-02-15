using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Authorization;

var builder = WebApplication.CreateBuilder(args);
// 📌 Yetkilendirme Servisi Ekleyelim
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/AuthUI/Login"; // **Yetkisiz kullanıcıları yönlendirecek sayfa**
        options.AccessDeniedPath = "/AuthUI/Login"; // **Erişimi olmayan sayfa**
    });

builder.Services.AddAuthorization();
// 📌 Session için gerekli servisleri ekleyelim
builder.Services.AddDistributedMemoryCache(); // Session için gerekli
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // 30 dakika boyunca session süresi
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 📌 MVC ve HttpContextAccessor eklenmeli
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor(); // HttpContextAccessor'ı DI container'a ekledik

var app = builder.Build();

// 📌 Middleware sırası önemli
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// 📌 Session middleware sırasını doğru yerleştir
app.UseSession(); // 📌 UseAuthorization'dan ÖNCE olmalı!
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
