using System.Text.Json;
using BoutiqueEnLigne.Models;

namespace BoutiqueEnLigne.Services
{
    public class FakeStoreService
    {
        private readonly HttpClient _httpClient;

        public FakeStoreService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<FakeStoreProduct>> GetProductsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://fakestoreapi.com/products");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var products = JsonSerializer.Deserialize<List<FakeStoreProduct>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return products ?? new List<FakeStoreProduct>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des produits: {ex.Message}");
                return new List<FakeStoreProduct>();
            }
        }
    }

    // Classe pour mapper les données de l'API FakeStore
    public class FakeStoreProduct
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
    }
}