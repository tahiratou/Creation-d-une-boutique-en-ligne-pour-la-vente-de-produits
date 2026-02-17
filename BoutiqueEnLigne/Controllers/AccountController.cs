using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BoutiqueEnLigne.Data;
using BoutiqueEnLigne.Models;
using BoutiqueEnLigne.ViewModels;

namespace BoutiqueEnLigne.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Vérifier si l'email existe déjà
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Cet email est déjà utilisé");
                    return View(model);
                }

                // Créer le nouvel utilisateur
                var user = new User
                {
                    Nom = model.Nom,
                    Prenom = model.Prenom,
                    Email = model.Email,
                    MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(model.MotDePasse),
                    Role = model.Role,
                    DateInscription = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Créer un panier pour le client
                if (user.Role == "Client")
                {
                    var panier = new Panier
                    {
                        UserId = user.Id
                    };
                    _context.Paniers.Add(panier);
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Inscription réussie ! Vous pouvez maintenant vous connecter.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        // GET: Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (user != null && BCrypt.Net.BCrypt.Verify(model.MotDePasse, user.MotDePasseHash))
                {
                    // Créer la session
                    HttpContext.Session.SetInt32("UserId", user.Id);
                    HttpContext.Session.SetString("UserEmail", user.Email);
                    HttpContext.Session.SetString("UserRole", user.Role);
                    HttpContext.Session.SetString("UserNom", $"{user.Prenom} {user.Nom}");

                    TempData["SuccessMessage"] = $"Bienvenue {user.Prenom} !";
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Email ou mot de passe incorrect");
            }

            return View(model);
        }

        // GET: Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Vous êtes déconnecté";
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Profile
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var user = await _context.Users
                .Include(u => u.Commandes)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }
    }
}