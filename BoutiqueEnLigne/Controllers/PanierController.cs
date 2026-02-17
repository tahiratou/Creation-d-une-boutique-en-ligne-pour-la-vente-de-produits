using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BoutiqueEnLigne.Data;
using BoutiqueEnLigne.Models;

namespace BoutiqueEnLigne.Controllers
{
    public class PanierController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PanierController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Panier
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Vous devez être connecté pour accéder au panier";
                return RedirectToAction("Login", "Account");
            }

            var panier = await _context.Paniers
                .Include(p => p.Items)
                    .ThenInclude(i => i.Produit)
                        .ThenInclude(pr => pr.Vendeur)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (panier == null)
            {
                // Créer un panier si l'utilisateur n'en a pas
                panier = new Panier { UserId = userId.Value };
                _context.Paniers.Add(panier);
                await _context.SaveChangesAsync();
            }

            return View(panier);
        }

        // POST: Panier/AjouterProduit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AjouterProduit(int produitId, int quantite = 1)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Vous devez être connecté pour ajouter au panier";
                return RedirectToAction("Login", "Account");
            }

            // Vérifier que le produit existe et a du stock
            var produit = await _context.Produits.FindAsync(produitId);
            if (produit == null)
            {
                TempData["ErrorMessage"] = "Produit introuvable";
                return RedirectToAction("Index", "Produits");
            }

            if (produit.Stock < quantite)
            {
                TempData["ErrorMessage"] = "Stock insuffisant";
                return RedirectToAction("Details", "Produits", new { id = produitId });
            }

            // Récupérer ou créer le panier
            var panier = await _context.Paniers
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (panier == null)
            {
                panier = new Panier { UserId = userId.Value };
                _context.Paniers.Add(panier);
                await _context.SaveChangesAsync();
            }

            // Vérifier si le produit est déjà dans le panier
            var itemExistant = panier.Items.FirstOrDefault(i => i.ProduitId == produitId);

            if (itemExistant != null)
            {
                // Augmenter la quantité
                itemExistant.Quantite += quantite;
            }
            else
            {
                // Ajouter un nouvel item
                var nouvelItem = new PanierItem
                {
                    PanierId = panier.Id,
                    ProduitId = produitId,
                    Quantite = quantite
                };
                _context.PanierItems.Add(nouvelItem);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Produit ajouté au panier avec succès !";
            return RedirectToAction("Index");
        }

        // POST: Panier/ModifierQuantite
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModifierQuantite(int itemId, int quantite)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Non connecté" });
            }

            var item = await _context.PanierItems
                .Include(i => i.Panier)
                .Include(i => i.Produit)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.Panier.UserId == userId);

            if (item == null)
            {
                return Json(new { success = false, message = "Article introuvable" });
            }

            if (quantite <= 0)
            {
                _context.PanierItems.Remove(item);
            }
            else if (quantite > item.Produit.Stock)
            {
                return Json(new { success = false, message = "Stock insuffisant" });
            }
            else
            {
                item.Quantite = quantite;
            }

            await _context.SaveChangesAsync();

            // Calculer le nouveau total
            var panier = await _context.Paniers
                .Include(p => p.Items)
                    .ThenInclude(i => i.Produit)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            var total = panier.Items.Sum(i => i.Produit.Prix * i.Quantite);

            return Json(new { success = true, total = total.ToString("N2") });
        }

        // POST: Panier/SupprimerItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SupprimerItem(int itemId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Vous devez être connecté";
                return RedirectToAction("Login", "Account");
            }

            var item = await _context.PanierItems
                .Include(i => i.Panier)
                .FirstOrDefaultAsync(i => i.Id == itemId && i.Panier.UserId == userId);

            if (item != null)
            {
                _context.PanierItems.Remove(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Article supprimé du panier";
            }

            return RedirectToAction("Index");
        }

        // POST: Panier/Vider
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Vider()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var panier = await _context.Paniers
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (panier != null && panier.Items.Any())
            {
                _context.PanierItems.RemoveRange(panier.Items);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Panier vidé";
            }

            return RedirectToAction("Index");
        }
    }
}