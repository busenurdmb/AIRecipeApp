namespace AIRecipeApp.UI.Models
{
    public class Recipe
    {
        public string? Id { get; set; }

        public string Title { get; set; }

    
        public List<string> Ingredients { get; set; }

      
        public string Instructions { get; set; }
    }
}
