using Microsoft.EntityFrameworkCore;
using BoutiqueEnLigne.Models;

namespace BoutiqueEnLigne.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Produit> Produits { get; set; }
        public DbSet<Panier> Paniers { get; set; }
        public DbSet<PanierItem> PanierItems { get; set; }
        public DbSet<Commande> Commandes { get; set; }
        public DbSet<CommandeItem> CommandeItems { get; set; }
        public DbSet<Facture> Factures { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Produit>()
                .Property(p => p.Prix)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Commande>()
                .Property(c => c.MontantTotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<CommandeItem>()
                .Property(ci => ci.PrixUnitaire)
                .HasPrecision(18, 2);



            modelBuilder.Entity<CommandeItem>()
                .HasOne(ci => ci.Produit)
                .WithMany(p => p.CommandeItems)
                .HasForeignKey(ci => ci.ProduitId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PanierItem>()
                .HasOne(pi => pi.Produit)
                .WithMany(p => p.PanierItems)
                .HasForeignKey(pi => pi.ProduitId)
                .OnDelete(DeleteBehavior.Restrict);

            
            modelBuilder.Entity<User>()
                .HasOne(u => u.Panier)
                .WithOne(p => p.User)
                .HasForeignKey<Panier>(p => p.UserId);
        }
    }
}
