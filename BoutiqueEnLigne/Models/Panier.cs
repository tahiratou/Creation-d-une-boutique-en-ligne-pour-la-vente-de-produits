namespace BoutiqueEnLigne.Models
{
    public class Panier
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        // Relations
        public virtual User? User { get; set; }
        public virtual ICollection<PanierItem> Items { get; set; } = new List<PanierItem>();

        // Propriété calculée
        public decimal MontantTotal => Items.Sum(i => (i.Produit?.Prix ?? 0) * i.Quantite);
    }

    public class PanierItem
    {
        public int Id { get; set; }
        public int PanierId { get; set; }
        public int ProduitId { get; set; }
        public int Quantite { get; set; }

        // Relations
        public virtual Panier? Panier { get; set; }
        public virtual Produit? Produit { get; set; }
    }
}