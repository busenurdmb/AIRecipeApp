using AIRecipeApp.Api.Entities;

namespace AIRecipeApp.Api.Interfaces
{
    public interface IRecipeService
    {
        Task<List<Recipe>> GetAllAsync();
        Task CreateAsync(Recipe recipe);
        Task<List<Recipe>> GetByUserIdAsync(string userId);
    }

}
