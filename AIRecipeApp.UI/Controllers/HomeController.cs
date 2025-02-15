using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AIRecipeApp.UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;

        public HomeController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetRecipe(string ingredients)
        {
            if (string.IsNullOrWhiteSpace(ingredients))
            {
                TempData["ErrorMessage"] = "Lütfen en az bir malzeme girin!";
                return RedirectToAction("Index");
            }

            // ?? Boþluklarý temizleyerek, doðrudan liste olarak gönderiyoruz
            var ingredientList = ingredients.Split(',').Select(i => i.Trim()).ToArray();

            // ?? API'nin beklediði JSON formatýný doðrudan dizi olarak gönderelim
            var jsonContent = new StringContent(JsonConvert.SerializeObject(ingredientList), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://localhost:7025/api/recipe/get-recipe", jsonContent);
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Tarif alýnýrken hata oluþtu: {errorMessage}";
                return RedirectToAction("Index");
            }

            var result = await response.Content.ReadAsStringAsync();
            ViewBag.RecipeResult = result;
            return View("Index");
        }


        [HttpPost]
        public async Task<IActionResult> SaveRecipe(string Title, string Ingredients, string Instructions)
        {
            var token = HttpContext.Session.GetString("auth_token");
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Tarif kaydetmek için giriþ yapmalýsýnýz!";
                return RedirectToAction("Index");
            }

            var recipe = new { Title, Ingredients = Ingredients.Split(','), Instructions };
            var jsonContent = new StringContent(JsonConvert.SerializeObject(recipe), Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://localhost:7025/api/Recipe/save-recipe"),
                Headers = { { "Authorization", $"Bearer {token}" } },
                Content = jsonContent
            };

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Tarif kaydedilemedi!";
                return RedirectToAction("Index");
            }

            TempData["SuccessMessage"] = "Tarif baþarýyla kaydedildi!";
            return RedirectToAction("Index");
        }
    }
}
