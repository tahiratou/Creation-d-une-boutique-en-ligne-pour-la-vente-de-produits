using System.ComponentModel.DataAnnotations;

namespace BoutiqueEnLigne.ViewModels
{
    public class PaiementViewModel
    {
        public decimal MontantTotal { get; set; }
        public List<PaiementItemViewModel> Articles { get; set; } = new();
        public string StripePublishableKey { get; set; } = string.Empty;
        public string? StripeToken { get; set; }

        [Required(ErrorMessage = "Le nom est requis")]
        public string NomDetenteur { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le code postal est requis")]
        public string CodePostal { get; set; } = string.Empty;
    }
    public class PaiementItemViewModel
    {
        public string Titre { get; set; } = string.Empty;
        public int Quantite { get; set; }
        public decimal PrixUnitaire { get; set; }
        public decimal SousTotal => PrixUnitaire * Quantite;
    }
}