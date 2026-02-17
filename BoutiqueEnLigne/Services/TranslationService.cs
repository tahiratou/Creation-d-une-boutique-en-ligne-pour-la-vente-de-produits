namespace BoutiqueEnLigne.Services
{
    public static class TranslationService
    {
        private static readonly Dictionary<string, string> CategoryTranslations = new()
        {
            { "electronics", "Électronique" },
            { "jewelery", "Bijoux" },
            { "men's clothing", "Vêtements homme" },
            { "women's clothing", "Vêtements femme" }
        };

        public static string TranslateCategory(string category)
        {
            if (string.IsNullOrEmpty(category))
                return "Non catégorisé";

            var lowerCategory = category.ToLower();
            return CategoryTranslations.TryGetValue(lowerCategory, out var translation)
                ? translation
                : category;
        }

        public static string FormatPrice(decimal price)
        {
            return $"{price:N2} $";
        }
    }
}