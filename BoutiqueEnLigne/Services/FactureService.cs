using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using BoutiqueEnLigne.Models;
using Microsoft.EntityFrameworkCore;
using BoutiqueEnLigne.Data;

namespace BoutiqueEnLigne.Services
{
    public class FactureService
    {
        private readonly ApplicationDbContext _context;

        public FactureService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> GenererFacturePDF(int factureId)
        {
            var facture = await _context.Factures
                .Include(f => f.Commande)
                    .ThenInclude(c => c.User)
                .Include(f => f.Commande)
                    .ThenInclude(c => c.Items)
                        .ThenInclude(i => i.Produit)
                            .ThenInclude(p => p.Vendeur)
                .FirstOrDefaultAsync(f => f.Id == factureId);

            if (facture == null)
                throw new Exception("Facture introuvable");

            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Header()
                        .Height(100)
                        .Background(Colors.Orange.Medium)
                        .Padding(20)
                        .Row(row =>
                        {
                            row.RelativeItem().Column(column =>
                            {
                                column.Item().Text("DevStore").FontSize(28).Bold().FontColor(Colors.White);
                                column.Item().Text("Boutique en ligne").FontSize(12).FontColor(Colors.White);
                            });

                            row.RelativeItem().AlignRight().Column(column =>
                            {
                                column.Item().Text("FACTURE").FontSize(24).Bold().FontColor(Colors.White);
                                column.Item().Text($"N° {facture.NumeroFacture}").FontSize(12).FontColor(Colors.White);
                            });
                        });

                    page.Content()
                        .PaddingVertical(20)
                        .Column(column =>
                        {
                            column.Spacing(20);

                            // Informations client et facture
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("FACTURÉ À :").Bold().FontSize(12);
                                    col.Item().PaddingTop(5).Text($"{facture.Commande.User?.Prenom} {facture.Commande.User?.Nom}");
                                    col.Item().Text($"{facture.Commande.User?.Email}");
                                });

                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("DÉTAILS DE LA FACTURE").Bold().FontSize(12);
                                    col.Item().PaddingTop(5).Text($"Date : {facture.DateGeneration:dd/MM/yyyy}");
                                    col.Item().Text($"Commande : #{facture.CommandeId}");
                                    col.Item().Text($"Statut : {facture.Commande.StatutPaiement}");
                                });
                            });

                            // Ligne de séparation
                            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                            // Tableau des produits
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                });

                                // En-tête
                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Background(Colors.Orange.Lighten3).Text("Produit").Bold();
                                    header.Cell().Element(CellStyle).Background(Colors.Orange.Lighten3).Text("Prix unit.").Bold();
                                    header.Cell().Element(CellStyle).Background(Colors.Orange.Lighten3).Text("Qté").Bold();
                                    header.Cell().Element(CellStyle).Background(Colors.Orange.Lighten3).Text("Total").Bold();

                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten1).Padding(5);
                                    }
                                });

                                // Lignes de produits
                                foreach (var item in facture.Commande.Items)
                                {
                                    table.Cell().Element(CellStyle).Text(item.Produit?.Titre);
                                    table.Cell().Element(CellStyle).Text($"{item.PrixUnitaire:N2} $");
                                    table.Cell().Element(CellStyle).Text(item.Quantite.ToString());
                                    table.Cell().Element(CellStyle).Text($"{item.PrixUnitaire * item.Quantite:N2} $").Bold();

                                    static IContainer CellStyle(IContainer container)
                                    {
                                        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5);
                                    }
                                }
                            });

                            // Totaux
                            column.Item().AlignRight().Column(col =>
                            {
                                col.Item().PaddingTop(10).Row(row =>
                                {
                                    row.RelativeItem().Text("Sous-total :").FontSize(12);
                                    row.RelativeItem().AlignRight().Text($"{facture.Commande.MontantTotal:N2} $").FontSize(12);
                                });

                                col.Item().Row(row =>
                                {
                                    row.RelativeItem().Text("Livraison :").FontSize(12);
                                    row.RelativeItem().AlignRight().Text("GRATUIT").FontSize(12).FontColor(Colors.Green.Medium);
                                });

                                col.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Medium);

                                col.Item().PaddingTop(5).Row(row =>
                                {
                                    row.RelativeItem().Text("TOTAL :").Bold().FontSize(14);
                                    row.RelativeItem().AlignRight().Text($"{facture.Commande.MontantTotal:N2} $").Bold().FontSize(14).FontColor(Colors.Orange.Medium);
                                });
                            });

                            // Informations de paiement
                            column.Item().PaddingTop(20).Background(Colors.Grey.Lighten3).Padding(10).Column(col =>
                            {
                                col.Item().Text("INFORMATIONS DE PAIEMENT").Bold().FontSize(12);
                                col.Item().PaddingTop(5).Text($"Méthode : Carte de crédit (Stripe)");
                                col.Item().Text($"Transaction ID : {facture.Commande.StripePaymentIntentId}");
                                col.Item().Text($"Date de paiement : {facture.Commande.DateCommande:dd/MM/yyyy à HH:mm}");
                            });

                            // Informations vendeurs
                            column.Item().PaddingTop(20).Column(col =>
                            {
                                col.Item().Text("VENDEURS").Bold().FontSize(12);
                                var vendeurs = facture.Commande.Items
                                    .Select(i => i.Produit?.Vendeur)
                                    .Where(v => v != null)
                                    .Distinct()
                                    .ToList();

                                foreach (var vendeur in vendeurs)
                                {
                                    col.Item().PaddingTop(5).Text($"• {vendeur?.Prenom} {vendeur?.Nom} - {vendeur?.Email}");
                                }
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text("DevStore - Boutique en ligne | contact@devstore.com | Merci de votre confiance !")
                        .FontSize(9)
                        .FontColor(Colors.Grey.Medium);
                });
            });

            return document.GeneratePdf();
        }
    }
}