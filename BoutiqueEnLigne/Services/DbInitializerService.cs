using BoutiqueEnLigne.Data;
using BoutiqueEnLigne.Models;
using BCrypt.Net;

namespace BoutiqueEnLigne.Services
{
    public class DbInitializerService
    {
        private readonly ApplicationDbContext _context;
        private readonly FakeStoreService _fakeStoreService;

        public DbInitializerService(ApplicationDbContext context, FakeStoreService fakeStoreService)
        {
            _context = context;
            _fakeStoreService = fakeStoreService;
        }

        public async Task InitializeAsync()
        {
            if (_context.Users.Any())
            {
                return;
            }

            // Créer des utilisateurs
            var vendeur1 = new User
            {
                Nom = "Dupont",
                Prenom = "Jean",
                Email = "vendeur@test.com",
                MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                Role = "Vendeur",
                DateInscription = DateTime.Now
            };

            var client1 = new User
            {
                Nom = "Martin",
                Prenom = "Marie",
                Email = "client@test.com",
                MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
                Role = "Client",
                DateInscription = DateTime.Now
            };

            _context.Users.AddRange(vendeur1, client1);
            await _context.SaveChangesAsync();

            var panier = new Panier { UserId = client1.Id };
            _context.Paniers.Add(panier);
            await _context.SaveChangesAsync();

            // Récupérer et traduire les produits
            var fakeStoreProducts = await _fakeStoreService.GetProductsAsync();

            foreach (var fakeProduct in fakeStoreProducts)
            {
                var produit = new Produit
                {
                    Titre = fakeProduct.Title, 
                    Description = fakeProduct.Description, 
                    Prix = fakeProduct.Price,
                    Categorie = fakeProduct.Category,
                    ImageUrl = fakeProduct.Image,
                    Stock = 10,
                    VendeurId = vendeur1.Id
                };
                _context.Produits.Add(produit);
            }

            await _context.SaveChangesAsync();
            Console.WriteLine("Base de données initialisée avec succès !");
        }
    }
}