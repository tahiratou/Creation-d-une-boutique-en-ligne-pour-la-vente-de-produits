namespace BoutiqueEnLigne.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string MotDePasseHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Client"; 
        public DateTime DateInscription { get; set; } = DateTime.Now;

        // Relations
        public virtual ICollection<Commande> Commandes { get; set; } = new List<Commande>();
        public virtual ICollection<Produit> ProduitsVendus { get; set; } = new List<Produit>();
        public virtual Panier? Panier { get; set; }  
    }
}