using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BoutiqueEnLigne.Data;
using BoutiqueEnLigne.Models;
using BoutiqueEnLigne.Attributes;

namespace BoutiqueEnLigne.Controllers
{
    [RoleAuthorize("Vendeur")]
    public class VendeurController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VendeurController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Vendeur/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var vendeurId = HttpContext.Session.GetInt32("UserId");

            var vendeur = await _context.Users
                .Include(u => u.ProduitsVendus)
                .FirstOrDefaultAsync(u => u.Id == vendeurId);

            // Calculer le total des ventes
            var ventes = await _context.CommandeItems
                .Include(ci => ci.Produit)
                .Where(ci => ci.Produit.VendeurId == vendeurId)
                .ToListAsync();

            var totalVentes = ventes.Sum(v => v.PrixUnitaire * v.Quantite);

            ViewBag.TotalVentes = totalVentes;
            ViewBag.NombreProduits = vendeur?.ProduitsVendus.Count ?? 0;
            ViewBag.VentesRecentes = ventes.Count;

            return View(vendeur);
        }

        // GET: Vendeur/MesProduits
        public async Task<IActionResult> MesProduits()
        {
            var vendeurId = HttpContext.Session.GetInt32("UserId");

            var produits = await _context.Produits
                .Where(p => p.VendeurId == vendeurId)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return View(produits);
        }

        // GET: Vendeur/AjouterProduit
        public IActionResult AjouterProduit()
        {
            return View();
        }

        // POST: Vendeur/AjouterProduit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AjouterProduit(Produit produit)
        {
            var vendeurId = HttpContext.Session.GetInt32("UserId");

            if (ModelState.IsValid)
            {
                produit.VendeurId = vendeurId.Value;
                _context.Produits.Add(produit);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Produit ajouté avec succès !";
                return RedirectToAction("MesProduits");
            }

            return View(produit);
        }

        // GET: Vendeur/ModifierProduit/5
        public async Task<IActionResult> ModifierProduit(int? id)
        {
            if (id == null) return NotFound();

            var vendeurId = HttpContext.Session.GetInt32("UserId");
            var produit = await _context.Produits
                .FirstOrDefaultAsync(p => p.Id == id && p.VendeurId == vendeurId);

            if (produit == null) return NotFound();

            return View(produit);
        }

        // POST: Vendeur/ModifierProduit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModifierProduit(int id, Produit produit)
        {
            if (id != produit.Id) return NotFound();

            var vendeurId = HttpContext.Session.GetInt32("UserId");
            var produitExistant = await _context.Produits
                .FirstOrDefaultAsync(p => p.Id == id && p.VendeurId == vendeurId);

            if (produitExistant == null) return NotFound();

            if (ModelState.IsValid)
            {
                produitExistant.Titre = produit.Titre;
                produitExistant.Description = produit.Description;
                produitExistant.Prix = produit.Prix;
                produitExistant.Categorie = produit.Categorie;
                produitExistant.ImageUrl = produit.ImageUrl;
                produitExistant.Stock = produit.Stock;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Produit modifié avec succès !";
                return RedirectToAction("MesProduits");
            }

            return View(produit);
        }

        // POST: Vendeur/SupprimerProduit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SupprimerProduit(int id)
        {
            var vendeurId = HttpContext.Session.GetInt32("UserId");
            var produit = await _context.Produits
                .FirstOrDefaultAsync(p => p.Id == id && p.VendeurId == vendeurId);

            if (produit != null)
            {
                _context.Produits.Remove(produit);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Produit supprimé avec succès !";
            }

            return RedirectToAction("MesProduits");
        }

        // GET: Vendeur/Ventes
        public async Task<IActionResult> Ventes()
        {
            var vendeurId = HttpContext.Session.GetInt32("UserId");

            var ventes = await _context.CommandeItems
                .Include(ci => ci.Produit)
                .Include(ci => ci.Commande)
                    .ThenInclude(c => c.User)
                .Where(ci => ci.Produit.VendeurId == vendeurId)
                .OrderByDescending(ci => ci.Commande.DateCommande)
                .ToListAsync();

            return View(ventes);
        }
    }
}