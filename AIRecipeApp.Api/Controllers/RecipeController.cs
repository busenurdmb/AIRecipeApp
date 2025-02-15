using AIRecipeApp.Api.Entities;
using AIRecipeApp.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
public class RecipeController : ControllerBase
{
    private readonly IRecipeService _recipeService;
    private readonly IOpenAiService _aiService;

    // Bağımlılıkları enjekte ederek tarif işlemleri ve OpenAI entegrasyonu için kullanılan controller.
    public RecipeController(IRecipeService recipeService, IOpenAiService aiService)
    {
        _recipeService = recipeService;
        _aiService = aiService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Recipe>>> Get()
    {
        // Tüm tarifleri getirir ve HTTP 200 (OK) olarak döner.
        var recipes = await _recipeService.GetAllAsync();
        return Ok(recipes);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Recipe recipe)
    {
        // Yeni bir tarif ekler. Eksik bilgi varsa HTTP 400 (Bad Request) döner.
        if (recipe == null)
            return BadRequest("Tarif bilgisi eksik!");

        await _recipeService.CreateAsync(recipe);
        return CreatedAtAction(nameof(Get), new { id = recipe.Id }, recipe);
    }

    [HttpPost("get-recipe")]
    public async Task<IActionResult> GetRecipeFromAI([FromBody] List<string> ingredients)
    {
        // Kullanıcının girdiği malzemelere göre OpenAI API'den yemek tarifi alır.
        if (ingredients == null || ingredients.Count == 0)
            return BadRequest("Lütfen en az bir malzeme girin.");

        var recipeText = await _aiService.GetRecipeFromAI(ingredients);
        var recipe = new Recipe
        {
            Title = "AI Önerisi",
            Ingredients = ingredients,
            Instructions = recipeText
        };

        return Ok(recipe);
    }

    [Authorize]
    [HttpGet("list")]
    public async Task<IActionResult> GetRecipes()
    {
        // Kullanıcının kayıtlı tariflerini getirir. Eğer kullanıcı giriş yapmamışsa HTTP 401 (Unauthorized) döner.
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        var recipes = await _recipeService.GetByUserIdAsync(userId);
        return Ok(recipes);
    }

    [Authorize]
    [HttpPost("save-recipe")]
    public async Task<IActionResult> SaveRecipe([FromBody] Recipe recipe)
    {
        // Kullanıcının yeni bir tarif kaydetmesini sağlar. Kullanıcı giriş yapmamışsa HTTP 401 (Unauthorized) döner.
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return Unauthorized();

        recipe.UserId = userId;
        await _recipeService.CreateAsync(recipe);
        return Ok(new { message = "Tarif başarıyla kaydedildi!", recipe });
    }
}
