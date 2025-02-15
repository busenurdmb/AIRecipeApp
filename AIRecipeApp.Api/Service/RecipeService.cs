using AIRecipeApp.Api.Context;
using AIRecipeApp.Api.Entities;
using AIRecipeApp.Api.Interfaces;
using MongoDB.Driver;

public class RecipeService : IRecipeService
{
    private readonly IMongoCollection<Recipe> _recipes;

    // MongoDB bağlantısını kurarak tarif koleksiyonunu hazırlar.
    public RecipeService(MongoDbContext context)
    {
        _recipes = context.Recipes;
    }

    public async Task<List<Recipe>> GetAllAsync()
    {
        // Tüm tarifleri veritabanından getirir.
        return await _recipes.Find(recipe => true).ToListAsync();
    }

    public async Task CreateAsync(Recipe recipe)
    {
        // Yeni bir tarifi veritabanına kaydeder.
        await _recipes.InsertOneAsync(recipe);
    }

    public async Task<List<Recipe>> GetByUserIdAsync(string userId)
    {
        // Kullanıcının kendi tariflerini getirir.
        return await _recipes.Find(r => r.UserId == userId).ToListAsync();
    }
}
