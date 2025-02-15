using AIRecipeApp.Api.Interfaces;
using Newtonsoft.Json;
using System.Text;

public class OpenAiService : IOpenAiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    // OpenAI API anahtarını alarak HTTP istemcisi başlatır.
    public OpenAiService(IConfiguration config)
    {
        _httpClient = new HttpClient();
        _apiKey = config["OpenAI:ApiKey"];
    }

    public async Task<string> GetRecipeFromAI(List<string> ingredients)
    {
        // Kullanıcının verdiği malzemelerle OpenAI'ye tarif isteği gönderir.
        var prompt = $"Elimde şu malzemeler var: {string.Join(", ", ingredients)}. Bana bu malzemelerle yapabileceğim en iyi yemek tarifini anlat.";

        var requestBody = new
        {
            model = "gpt-4o-mini",
            messages = new[] { new { role = "user", content = prompt } }
        };

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri("https://api.openai.com/v1/chat/completions"),
            Headers = { { "Authorization", $"Bearer {_apiKey}" } },
            Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json")
        };

        var response = await _httpClient.SendAsync(request);
        var result = await response.Content.ReadAsStringAsync();

        Console.WriteLine("OpenAI Response: " + result); // API'den dönen JSON çıktısını konsolda gösterir.

        // API yanıtını işleyerek tarif metnini döndürür.
        return JsonConvert.DeserializeObject<dynamic>(result)["choices"][0]["message"]["content"].ToString();
    }
}
