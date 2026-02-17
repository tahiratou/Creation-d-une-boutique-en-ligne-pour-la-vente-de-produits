using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BoutiqueEnLigne.Data;

namespace BoutiqueEnLigne.ViewComponents
{
    public class PanierCountViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public PanierCountViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return View(0);
            }

            var panier = await _context.Paniers
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            var count = panier?.Items.Sum(i => i.Quantite) ?? 0;

            return View(count);
        }
    }
}