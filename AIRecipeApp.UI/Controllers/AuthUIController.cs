using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace AIRecipeApp.UI.Controllers
{
    
    public class AuthUIController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthUIController(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Kullanıcı adı ve şifre zorunludur.";
                return View();
            }

            var loginData = new { Username = username, Password = password };
            var jsonContent = new StringContent(JsonConvert.SerializeObject(loginData), Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.PostAsync("https://localhost:7025/api/auth/login", jsonContent);
            }
            catch (HttpRequestException)
            {
                ViewBag.Error = "Sunucuya bağlanılamadı. Lütfen daha sonra tekrar deneyin.";
                return View();
            }

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Kullanıcı adı veya şifre hatalı!";
                return View();
            }

            var result = await response.Content.ReadAsStringAsync();
            var jsonObj = JsonConvert.DeserializeObject<JObject>(result);
            var token = jsonObj?["token"]?.ToString();

            if (string.IsNullOrEmpty(token) || _httpContextAccessor.HttpContext == null)
            {
                ViewBag.Error = "Giriş sırasında beklenmeyen bir hata oluştu.";
                return View();
            }

            _httpContextAccessor.HttpContext.Session.SetString("auth_token", token);
            return RedirectToAction("MyRecipes", "RecipeUI");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            _httpContextAccessor.HttpContext?.Session.Remove("auth_token");
            return RedirectToAction("Login","AuthUI");
        }
    }
}
