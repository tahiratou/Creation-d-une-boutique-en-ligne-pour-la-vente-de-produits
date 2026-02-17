namespace BoutiqueEnLigne.Models
{
    public class Produit
    {
        public int Id { get; set; }
        public string Titre { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Prix { get; set; }
        public string Categorie { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int Stock { get; set; } = 0;
        public int VendeurId { get; set; }

        // Relations
        public virtual User? Vendeur { get; set; }
        public virtual ICollection<PanierItem> PanierItems { get; set; } = new List<PanierItem>();  
        public virtual ICollection<CommandeItem> CommandeItems { get; set; } = new List<CommandeItem>();  
    }
}