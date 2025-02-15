using Microsoft.AspNetCore.Mvc;
using System.Text;
using Newtonsoft.Json;
using AIRecipeApp.UI.Models;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
namespace AIRecipeApp.UI.Controllers
{
   
    public class RecipeUIController : Controller
    {
        private readonly HttpClient _httpClient;

        public RecipeUIController()
        {
            _httpClient = new HttpClient();
        }

        // 📌 Ana Sayfa
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> MyRecipes()
        {
            var token = HttpContext.Session.GetString("auth_token");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "AuthUI");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync("https://localhost:7025/api/Recipe/list");

            if (!response.IsSuccessStatusCode)
                return Unauthorized();

            var result = await response.Content.ReadAsStringAsync();
            var recipes = JsonConvert.DeserializeObject<List<Recipe>>(result);

            return View(recipes);
        }

        // 📌 Ana Sayfa
        public async Task<IActionResult> SavedRecipes()
        {
            var response = await _httpClient.GetAsync("https://localhost:7025/api/Recipe");
            var result = await response.Content.ReadAsStringAsync();
            var recipes = JsonConvert.DeserializeObject<List<Recipe>>(result);

            return View(recipes);
        }
        // 📌 OpenAI'den tarif al ve göster
        [HttpPost]
        public async Task<IActionResult> GetRecipe(string ingredients)
        {
            var ingredientList = ingredients.Split(',');
            var jsonContent = new StringContent(JsonConvert.SerializeObject(ingredientList), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://localhost:7025/api/recipe/get-recipe", jsonContent);
            var result = await response.Content.ReadAsStringAsync();

            ViewBag.RecipeResult = result;
            return View("Index");
        }

        [HttpPost]
        public async Task<IActionResult> SaveRecipe(CreateRecibe recipe)
        {
            if (recipe == null || string.IsNullOrEmpty(recipe.Title))
            {
                TempData["ErrorMessage"] = "Tarif bilgisi eksik!";
                return RedirectToAction("Index");
            }

            // 📌 Kullanıcı oturumunu kontrol et
            var token = HttpContext.Session.GetString("auth_token");
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Oturumunuz sona erdi, lütfen tekrar giriş yapın.";
                return RedirectToAction("Login", "AuthUI");
            }

            // 📌 Kullanıcı ID'yi token'dan al
        
        

            try
            {
                // 📌 API'nin beklediği formatı eksiksiz oluştur
                var requestBody = JsonConvert.SerializeObject(new
                {
                   
                    title = recipe.Title,
                    ingredients = recipe.Ingredients ?? new List<string>(),  // Boş olursa default değer ata
                    instructions = recipe.Instructions ?? "Talimat verilmedi.",
                    userId = "1a" // 📌 API'nin beklediği UserId
                });

                Console.WriteLine("Request JSON: " + requestBody); // Debug için log yazdır

                var jsonContent = new StringContent(requestBody, Encoding.UTF8, "application/json");

                // 📌 API'ye JWT ekle
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsync("https://localhost:7025/api/Recipe/save-recipe", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Tarif başarıyla kaydedildi!";
                    return RedirectToAction("Index");
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Tarif kaydedilemedi. API Hatası: {response.StatusCode} - {errorMsg}";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Beklenmedik bir hata oluştu: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

    }
}
