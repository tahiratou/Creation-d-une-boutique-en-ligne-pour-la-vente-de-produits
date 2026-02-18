using BoutiqueEnLigne.Attributes;
using BoutiqueEnLigne.Data;
using BoutiqueEnLigne.Models;
using BoutiqueEnLigne.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;

namespace BoutiqueEnLigne.Controllers
{

    [RoleAuthorize("Client")]

    public class PaiementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public PaiementController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: Paiement/Index
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Vous devez être connecté";
                return RedirectToAction("Login", "Account");
            }

            // Récupérer le panier
            var panier = await _context.Paniers
                .Include(p => p.Items)
                    .ThenInclude(i => i.Produit)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (panier == null || !panier.Items.Any())
            {
                TempData["ErrorMessage"] = "Votre panier est vide";
                return RedirectToAction("Index", "Panier");
            }

            var viewModel = new PaiementViewModel
            {
                MontantTotal = panier.Items.Sum(i => i.Produit.Prix * i.Quantite),
                StripePublishableKey = _configuration["Stripe:PublishableKey"] ?? "",
                Articles = panier.Items.Select(i => new PaiementItemViewModel
                {
                    Titre = i.Produit.Titre,
                    Quantite = i.Quantite,
                    PrixUnitaire = i.Produit.Prix
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: Paiement/Traiter
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Traiter(string stripeToken)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Récupérer le panier
            var panier = await _context.Paniers
                .Include(p => p.Items)
                    .ThenInclude(i => i.Produit)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (panier == null || !panier.Items.Any())
            {
                TempData["ErrorMessage"] = "Votre panier est vide";
                return RedirectToAction("Index", "Panier");
            }

            var montantTotal = panier.Items.Sum(i => i.Produit.Prix * i.Quantite);

            try
            {
                // Créer le paiement avec Stripe
                var chargeOptions = new ChargeCreateOptions
                {
                    Amount = (long)(montantTotal * 100), // Stripe utilise les centimes
                    Currency = "cad",
                    Description = $"Commande DevStore - UserId:{userId}",
                    Source = stripeToken,
                };

                var chargeService = new ChargeService();
                var charge = await chargeService.CreateAsync(chargeOptions);

                if (charge.Status == "succeeded")
                {
                    // Créer la commande
                    var commande = new Commande
                    {
                        UserId = userId.Value,
                        DateCommande = DateTime.Now,
                        MontantTotal = montantTotal,
                        StatutPaiement = "Payé",
                        StripePaymentIntentId = charge.Id,
                        Items = panier.Items.Select(i => new CommandeItem
                        {
                            ProduitId = i.ProduitId,
                            Quantite = i.Quantite,
                            PrixUnitaire = i.Produit.Prix
                        }).ToList()
                    };

                    _context.Commandes.Add(commande);
                    await _context.SaveChangesAsync();

                    // Générer la facture
                    var facture = new Facture
                    {
                        CommandeId = commande.Id,
                        DateGeneration = DateTime.Now,
                        NumeroFacture = $"FACT-{DateTime.Now:yyyyMMdd}-{commande.Id:D4}"
                    };

                    _context.Factures.Add(facture);

                    // Mettre à jour le stock
                    foreach (var item in panier.Items)
                    {
                        var produit = await _context.Produits.FindAsync(item.ProduitId);
                        if (produit != null)
                        {
                            produit.Stock -= item.Quantite;
                        }
                    }

                    // Vider le panier
                    _context.PanierItems.RemoveRange(panier.Items);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Paiement effectué avec succès !";
                    return RedirectToAction("Confirmation", new { commandeId = commande.Id });
                }
                else
                {
                    TempData["ErrorMessage"] = "Le paiement a échoué. Veuillez réessayer.";
                    return RedirectToAction("Index");
                }
            }
            catch (StripeException e)
            {
                TempData["ErrorMessage"] = $"Erreur de paiement: {e.StripeError.Message}";
                return RedirectToAction("Index");
            }
        }

        // GET: Paiement/Confirmation
        public async Task<IActionResult> Confirmation(int commandeId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var commande = await _context.Commandes
                .Include(c => c.Items)
                    .ThenInclude(i => i.Produit)
                .Include(c => c.Facture)
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == commandeId && c.UserId == userId);

            if (commande == null)
            {
                return NotFound();
            }

            return View(commande);
        }
    }
}