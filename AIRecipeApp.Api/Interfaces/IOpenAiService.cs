namespace AIRecipeApp.Api.Interfaces
{
    public interface IOpenAiService
    {
        Task<string> GetRecipeFromAI(List<string> ingredients);
    }

}
