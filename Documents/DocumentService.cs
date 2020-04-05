using KalosfideAPI.Catalogues;
using KalosfideAPI.Catégories;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.DétailCommandes;
using KalosfideAPI.Partages;
using KalosfideAPI.Produits;
using KalosfideAPI.Utiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Documents
{
    class DataDétailProduit
    {
        public DétailCommandeData Détail { get; set; }
        public ProduitData Produit { get; set; }
        public CatégorieData Catégorie { get; set; }
    }

    public class DocumentService : BaseService, IDocumentService
    {
        private readonly IUtileService _utile;
        private readonly ICatalogueService _catalogueService;
        private readonly IDétailCommandeService _détailCommandeService;

        public DocumentService(ApplicationContext _context,
            IUtileService utile, ICatalogueService catalogueService, IDétailCommandeService détailCommandeService) : base(_context)
        {
            _utile = utile;
            _catalogueService = catalogueService;
            _détailCommandeService = détailCommandeService;
        }

        private DocumentCommande DocumentCommandeBase(Commande c)
        {
            DocumentCommande doc = new DocumentCommande
            {
                Uid = c.Uid,
                Rno = c.Rno,
                No = c.No,
                Date = c.Date,
                LivraisonNo = c.Livraison.No,
                DateLivraison = c.Livraison.Date,
            };
            return doc;
        }
        private DocumentCommande DocumentCommandeBase(DocumentCommande documentCommande)
        {
            DocumentCommande doc = new DocumentCommande
            {
                Uid = documentCommande.Uid,
                Rno = documentCommande.Rno,
                No = documentCommande.No,
                Date = documentCommande.Date,
                LivraisonNo = documentCommande.LivraisonNo,
                DateLivraison = documentCommande.DateLivraison,
            };
            return doc;
        }
        private async Task<DocumentCommande> DocumentCommandeTarif(AKeyUidRno keySite, Commande commande, Func<DétailCommande, bool> filtreDétails, bool avecDétails)
        {
            DocumentCommande documentCommande = DocumentCommandeBase(commande);
            IEnumerable<DétailCommande> détails = filtreDétails == null ? commande.Détails : commande.Détails.Where(d => filtreDétails(d)).ToList();
            if (avecDétails)
            {
                documentCommande.Détails = détails.Select(d => _détailCommandeService.DétailCommandeData(d)).ToList();
            }
            documentCommande.Tarif = await _catalogueService.Tarif(keySite, commande.Date.Value, détails);
            return documentCommande;
        }

        private async Task<List<DocumentCommande>> DocumentCommandes(AKeyUidRno keySite, List<Commande> commandes, Func<DétailCommande, bool> filtreDétails, bool avecDétails)
        {
            
            var y = await Task.WhenAll(commandes.Select(cl => DocumentCommandeTarif(keySite, cl, filtreDétails, avecDétails)));
            return y.ToList();
        }
        private DocumentCommande DocumentCommandeBilan(DocumentCommande documentCommandeTarif)
        {
            DocumentCommande documentCommande = DocumentCommandeBase(documentCommandeTarif);
            IEnumerable<DétailCommandeData> détails = documentCommandeTarif.Détails.Where(d => d.Demande.HasValue && d.Demande.Value > 0);
            documentCommande.LignesC = détails.Count();
            documentCommande.TotalC = _détailCommandeService.CoûtDemande(détails, documentCommandeTarif.Tarif, out bool incomplet);
            if (incomplet)
            {
                documentCommande.IncompletC = true;
            }
            détails = documentCommandeTarif.Détails.Where(d => d.ALivrer.HasValue && d.ALivrer.Value > 0);
            documentCommande.LignesL = détails.Count();
            documentCommande.TotalL = _détailCommandeService.CoûtALivrer(détails, documentCommandeTarif.Tarif);
            return documentCommande;
        }

        private DocumentBilan DocumentLivraisonBase(Livraison livraison, IEnumerable<DocumentCommande> commandes)
        {
            DocumentCommande commande = commandes.First();
            DocumentBilan doc = new DocumentBilan
            {
                Uid = commande.Uid,
                Rno = commande.Rno,
                No = livraison.No,
                Date = livraison.Date.Value,
            };
            return doc;
        }

        private DocumentBilan DocumentFactureBase(Facture facture)
        {
            DocumentBilan doc = new DocumentBilan
            {
                Uid = facture.Uid,
                Rno = facture.Rno,
                No = facture.No,
                Date = facture.Date.Value,
            };
            return doc;
        }

        private BilanProduit BilanProduit(ProduitData produit, IEnumerable<DétailCommandeData> détails, Func<decimal, DétailCommandeData, decimal> agrégeDétails)
        {
            decimal zéro = 0;
            return new BilanProduit
            {
                No = produit.No,
                Date = produit.Date,
                Total = détails.Aggregate(zéro, agrégeDétails)
            };
        }

        private DocumentBilan DocumentBilanTarif(DocumentBilan doc, List<DocumentCommande> commandes, Func<decimal, DétailCommandeData, decimal> agrégeDétails)
        {
            Func<DétailCommandeData, DocumentCommande, DataDétailProduit> détail_Produit =
                (détail, commande) =>
                {
                    ProduitData produit = commande.Tarif.Produits.Where(p => p.No == détail.No).First();
                    CatégorieData catégorie = commande.Tarif.Catégories.Where(c => c.No == produit.CategorieNo).First();
                    return new DataDétailProduit
                    {
                        Détail = détail,
                        Produit = produit,
                        Catégorie = catégorie
                    };
                };
            Func<IEnumerable<DataDétailProduit>, DocumentCommande, IEnumerable<DataDétailProduit>> agrége =
                (détails, commande) => détails.Concat(commande.Détails.Select(d => détail_Produit(d, commande)));
            var x = commandes
                .Aggregate(new List<DataDétailProduit>(), agrége)
                .GroupBy(dp => new { dp.Produit.No, dp.Produit.Date })
                .Select(dps => new { produit = dps.First().Produit, catégorie = dps.First().Catégorie, détails = dps.Select(dp => dp.Détail) });
            List<ProduitData> produits = x.Select(pcds => pcds.produit).ToList();
            List<CatégorieData> catégories = x.Select(pcds => pcds.catégorie)
                .GroupBy(c => new { c.No, c.Date })
                .Select(cs => cs.First())
                .ToList();
            doc.Produits = x.Select(pcds => BilanProduit(pcds.produit, pcds.détails, agrégeDétails)).ToList();
            doc.Tarif = new Catalogue
            {
                Produits = produits,
                Catégories = catégories
            };
            return doc;
        }

        private List<DocumentBilan> DocumentsLivraisonTarif(List<Commande> commandesAvecLivraison, List<DocumentCommande> documentCommandes)
        {
            IEnumerable<Livraison> livraisons = commandesAvecLivraison
                .GroupBy(c => c.Livraison)
                .Select(g => g.Key);
            List<DocumentBilan> docs = livraisons
                .Select(livraison => DocumentBilanTarif(DocumentLivraisonBase(livraison, documentCommandes), documentCommandes, (al, d) => al + d.ALivrer.Value))
                .ToList();
            return docs;
        }

        private List<DocumentBilan> DocumentLivraisonBilan(List<Commande> commandesAvecLivraison, List<DocumentCommande> documentCommandes)
        {
            IEnumerable<Livraison> livraisons = commandesAvecLivraison
                .GroupBy(c => c.Livraison)
                .Select(g => g.Key);
            var x = livraisons
                .Select(livraison => new {
                    document = DocumentLivraisonBase(livraison, documentCommandes),
                    tarif = DocumentBilanTarif(DocumentLivraisonBase(livraison, documentCommandes), documentCommandes, (al, d) => al + d.ALivrer.Value)
                })
                .ToList();
            Func<DocumentBilan, DocumentBilan, DocumentBilan> créeBilan =
                (DocumentBilan doc, DocumentBilan avecTarif) =>
                {
                    doc.Lignes = avecTarif.Produits.Count();
                    decimal zéro = 0;
                    doc.Total = avecTarif.Produits.Aggregate(zéro, (total, bp) =>
                      {
                          ProduitData produitData = avecTarif.Tarif.Produits.Where(p => p.No == bp.No && p.Date == bp.Date).First();
                          return total + produitData.Prix.Value * bp.Total;
                      });
                    return doc;

                };
            List<DocumentBilan> documents = x.Select(dt => créeBilan(dt.document, dt.tarif))
                .ToList();
            return documents;
        }

        private List<DocumentBilan> DocumentsFactureTarif(List<Commande> commandesAvecFacture, List<DocumentCommande> documentCommandes)
        {
            IEnumerable<Facture> factures = commandesAvecFacture
                .GroupBy(c => c.Livraison.Facture)
                .Select(g => g.Key);
            List<DocumentBilan> docs = factures
                .Select(facture => DocumentBilanTarif(DocumentFactureBase(facture), documentCommandes, (al, d) => al + d.ALivrer.Value))
                .ToList();
            return docs;
        }

        private List<DocumentBilan> DocumentFactureBilan(List<Commande> commandesAvecFacture, List<DocumentCommande> documentCommandes)
        {
            IEnumerable<Facture> factures = commandesAvecFacture
                .GroupBy(c => c.Livraison.Facture)
                .Select(g => g.Key);
            var x = factures
                .Select(facture => new {
                    document = DocumentFactureBase(facture),
                    tarif = DocumentBilanTarif(DocumentFactureBase(facture), documentCommandes, (af, d) => af + d.AFacturer.Value)
                })
                .ToList();
            Func<DocumentBilan, DocumentBilan, DocumentBilan> créeBilan =
                (DocumentBilan doc, DocumentBilan avecTarif) =>
                {
                    doc.Lignes = avecTarif.Produits.Count();
                    decimal zéro = 0;
                    doc.Total = avecTarif.Produits.Aggregate(zéro, (total, bp) =>
                      {
                          ProduitData produitData = avecTarif.Tarif.Produits.Where(p => p.No == bp.No && p.Date == bp.Date).First();
                          return total + produitData.Prix.Value * bp.Total;
                      });
                    return doc;

                };
            List<DocumentBilan> documents = x.Select(dt => créeBilan(dt.document, dt.tarif))
                .ToList();
            return documents;
        }

        private async Task<Documents> Liste(AKeyUidRno keySite, List<Commande> commandes)
        {

            List<DocumentCommande> commandesAvecTarif = await DocumentCommandes(keySite, commandes, filtreDétails: null, avecDétails: false);

            return new Documents
            {
                Commandes = commandesAvecTarif.Select(cat => DocumentCommandeBilan(cat)).ToList(),
                Livraisons = DocumentLivraisonBilan(commandes, commandesAvecTarif),
                Factures = DocumentFactureBilan(commandes, commandesAvecTarif)
            };
        }

        public async Task<Documents> ListeC(AKeyUidRno keySite, AKeyUidRno keyClient)
        {
            bool filtreCommandes(Commande c) => c.Uid == keyClient.Uid && c.Rno == keyClient.Rno && c.Date.HasValue;
            List<Commande> commandes = await _utile.CommandesAvecDétailsLivraisonEtFacture(filtreCommandes, null, null, null).ToListAsync();
            return await Liste(keySite, commandes);
        }

        public async Task<Documents> ListeF(AKeyUidRno keySite)
        {
            bool filtreCommandes(Commande c) => c.Date.HasValue;
            List<Commande> commandes = await _utile.CommandesAvecDétailsLivraisonEtFacture(filtreCommandes, null, null, keySite).ToListAsync();
            return await Liste(keySite, commandes);
        }

        public async Task<AKeyUidRnoNo> Commande(AKeyUidRno keySite, KeyUidRnoNo keyDocument)
        {
            bool filtreCommandes(Commande c) => c.Uid == keyDocument.Uid && c.Rno == keyDocument.Rno && c.No == keyDocument.No;
            bool filtreDétails(DétailCommande d) => d.Demande.HasValue;
            List<Commande> commandes = await _utile.CommandesAvecDétailsLivraisonEtFacture(filtreCommandes, filtreDétails, null, null).ToListAsync();
            if (commandes.Count() == 0)
            {
                return null;
            }
            DocumentCommande commande = (await DocumentCommandes(keySite, commandes, filtreDétails, avecDétails: true)).First();
            commande.Détails = commande.Détails.Select(d => new DétailCommandeData
            {
                No = d.No,
                TypeCommande = d.TypeCommande,
                Demande = d.Demande
            }).ToList();
            return commande;
        }

        public async Task<AKeyUidRnoNo> Livraison(AKeyUidRno keySite, KeyUidRnoNo keyDocument)
        {
            Func<Commande, bool> filtreCommandes = c => c.Uid == keyDocument.Uid && c.Rno == keyDocument.Rno;
            Func<DétailCommande, bool> filtreDétails = d => d.ALivrer.HasValue && d.ALivrer.Value > 0;
            List<Commande> commandes = await _utile.CommandesAvecDétailsLivraisonEtFacture(filtreCommandes, filtreDétails, null, null)
                .Where(c => c.Livraison != null && c.Livraison.No == keyDocument.No && c.Livraison.Date.HasValue)
                .ToListAsync();
            if (commandes.Count() == 0)
            {
                return null;
            }
            List<DocumentCommande> documentCommandes = (await DocumentCommandes(keySite, commandes, filtreDétails, avecDétails: true));
            DocumentBilan livraison = DocumentsLivraisonTarif(commandes, documentCommandes).First();
            return livraison;
        }

        public async Task<AKeyUidRnoNo> Facture(AKeyUidRno keySite, KeyUidRnoNo keyDocument)
        {
            Func<Commande, bool> filtreCommandes = c => c.Uid == keyDocument.Uid && c.Rno == keyDocument.Rno;
            Func<DétailCommande, bool> filtreDétails = d => d.AFacturer.HasValue && d.AFacturer.Value > 0;
            List<Commande> commandes = await _utile.CommandesAvecDétailsLivraisonEtFacture(filtreCommandes, filtreDétails, null, null)
                .Where(c => c.Livraison != null && c.Livraison.Facture != null && c.Livraison.Facture.No == keyDocument.No && c.Livraison.Facture.Date.HasValue)
                .ToListAsync();
            if (commandes.Count() == 0)
            {
                return null;
            }
            List<DocumentCommande> documents = await DocumentCommandes(keySite, commandes, filtreDétails, avecDétails: true);
            return DocumentsFactureTarif(commandes,documents).First();
        }
    }
}