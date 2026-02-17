namespace BoutiqueEnLigne.Models
{
    public class Facture
    {
        public int Id { get; set; }
        public int CommandeId { get; set; }
        public DateTime DateGeneration { get; set; } = DateTime.Now;
        public string NumeroFacture { get; set; } = string.Empty;

        // Relations
        public virtual Commande? Commande { get; set; }
    }
}