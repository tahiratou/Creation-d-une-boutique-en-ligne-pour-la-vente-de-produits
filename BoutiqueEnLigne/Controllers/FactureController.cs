using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BoutiqueEnLigne.Data;
using BoutiqueEnLigne.Services;

namespace BoutiqueEnLigne.Controllers
{
    public class FactureController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly FactureService _factureService;

        public FactureController(ApplicationDbContext context, FactureService factureService)
        {
            _context = context;
            _factureService = factureService;
        }

        // GET: Facture/Telecharger/5
        public async Task<IActionResult> Telecharger(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var facture = await _context.Factures
                .Include(f => f.Commande)
                .FirstOrDefaultAsync(f => f.Id == id && f.Commande.UserId == userId);

            if (facture == null)
            {
                return NotFound();
            }

            try
            {
                var pdfBytes = await _factureService.GenererFacturePDF(id);
                return File(pdfBytes, "application/pdf", $"Facture_{facture.NumeroFacture}.pdf");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Erreur lors de la génération de la facture : {ex.Message}";
                return RedirectToAction("Profile", "Account");
            }
        }

        // GET: Facture/MesFactures
        public async Task<IActionResult> MesFactures()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var factures = await _context.Factures
                .Include(f => f.Commande)
                    .ThenInclude(c => c.Items)
                .Where(f => f.Commande.UserId == userId)
                .OrderByDescending(f => f.DateGeneration)
                .ToListAsync();

            return View(factures);
        }
    }
}