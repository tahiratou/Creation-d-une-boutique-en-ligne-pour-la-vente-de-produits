namespace BoutiqueEnLigne.Services
{
    public static class ProductTranslationService
    {
        // Cache de traductions communes pour éviter les répétitions
        private static readonly Dictionary<string, string> CommonTranslations = new()
        {
            // Matériaux
            { "stainless steel", "acier inoxydable" },
            { "rose gold plated", "plaqué or rose" },
            { "gold plated", "plaqué or" },
            { "silver plated", "plaqué argent" },
            { "leather", "cuir" },
            { "cotton", "coton" },
            { "polyester", "polyester" },
            { "denim", "denim" },
            { "wool", "laine" },
            
            // Types de produits
            { "backpack", "sac à dos" },
            { "jacket", "veste" },
            { "shirt", "chemise" },
            { "t-shirt", "t-shirt" },
            { "earrings", "boucles d'oreilles" },
            { "necklace", "collier" },
            { "bracelet", "bracelet" },
            { "ring", "bague" },
            { "watch", "montre" },
            
            // Adjectifs
            { "casual", "décontracté" },
            { "slim fit", "coupe ajustée" },
            { "classic", "classique" },
            { "vintage", "vintage" },
            { "modern", "moderne" },
            { "comfortable", "confortable" },
            { "elegant", "élégant" },
            
            // Phrases communes
            { "made of", "fabriqué en" },
            { "made in", "fabriqué en" },
            { "great for", "idéal pour" },
            { "perfect for", "parfait pour" },
            { "your perfect pack", "votre sac parfait" },
            { "for everyday use", "pour un usage quotidien" },

            {"mens", "Hommes" },
            {"White", "Blancs" },

        };

        public static string TranslateText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var translatedText = text;

            // Appliquer les traductions communes (insensible à la casse)
            foreach (var translation in CommonTranslations)
            {
                translatedText = System.Text.RegularExpressions.Regex.Replace(
                    translatedText,
                    translation.Key,
                    translation.Value,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
            }

            return translatedText;
        }
    }
}