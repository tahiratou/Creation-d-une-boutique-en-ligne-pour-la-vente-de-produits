namespace BoutiqueEnLigne.Models
{
    public class Commande
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime DateCommande { get; set; } = DateTime.Now;
        public decimal MontantTotal { get; set; }
        public string StatutPaiement { get; set; } = "En attente";
        public string? StripePaymentIntentId { get; set; }

        // Relations
        public virtual User? User { get; set; }
        public virtual ICollection<CommandeItem> Items { get; set; } = new List<CommandeItem>();
        public virtual Facture? Facture { get; set; }
    }

    public class CommandeItem
    {
        public int Id { get; set; }
        public int CommandeId { get; set; }
        public int ProduitId { get; set; }
        public int Quantite { get; set; }
        public decimal PrixUnitaire { get; set; }

        // Relations
        public virtual Commande? Commande { get; set; }
        public virtual Produit? Produit { get; set; }
    }
}