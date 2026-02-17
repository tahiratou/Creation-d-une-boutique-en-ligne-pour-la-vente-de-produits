using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BoutiqueEnLigne.Data;
using BoutiqueEnLigne.Models;

namespace BoutiqueEnLigne.Controllers
{
    public class ProduitsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProduitsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Produits
        public async Task<IActionResult> Index(string? categorie, string? recherche)
        {
            var produitsQuery = _context.Produits.Include(p => p.Vendeur).AsQueryable();

            // Filtrer par catégorie
            if (!string.IsNullOrEmpty(categorie))
            {
                produitsQuery = produitsQuery.Where(p => p.Categorie == categorie);
            }

            // Recherche par titre ou description
            if (!string.IsNullOrEmpty(recherche))
            {
                produitsQuery = produitsQuery.Where(p =>
                    p.Titre.Contains(recherche) ||
                    p.Description.Contains(recherche));
            }

            var produits = await produitsQuery.ToListAsync();

            // Récupérer toutes les catégories pour le filtre
            ViewBag.Categories = await _context.Produits
                .Select(p => p.Categorie)
                .Distinct()
                .ToListAsync();

            ViewBag.CategorieSelectionnee = categorie;
            ViewBag.Recherche = recherche;

            return View(produits);
        }

        // GET: Produits/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var produit = await _context.Produits
                .Include(p => p.Vendeur)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (produit == null)
            {
                return NotFound();
            }

            return View(produit);
        }
    }
}