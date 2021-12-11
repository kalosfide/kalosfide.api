using KalosfideAPI.Catalogues;
using KalosfideAPI.Catégories;
using KalosfideAPI.Clients;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages;
using KalosfideAPI.Produits;
using KalosfideAPI.Roles;
using KalosfideAPI.Utiles;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.CLF
{
    public class CLFService: BaseService, ICLFService
    {
        private readonly IUtileService _utile;
        private readonly ICatalogueService _catalogue;
        private readonly IProduitService _produitService;
        private readonly ICatégorieService _catégorieService;
        private readonly IClientService _clientService;

        public CLFService(ApplicationContext context, IUtileService utile, ICatalogueService catalogue,
            IProduitService produitService, ICatégorieService catégorieService,
            IClientService clientService): base(context)
        {
            _utile = utile;
            _catalogue = catalogue;
            _produitService = produitService;
            _catégorieService = catégorieService;
            _clientService = clientService;
        }

        #region Utile

        public DateTime DateNulle { get {
                return new DateTime(0);
            }
        }

        /// <summary>
        /// Retourne le document défini par la clé et le type avec ses lignes.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<DocCLF> DocCLFDeKey(AKeyUidRnoNo doc, string type)
        {
            return await _context.Docs
                .Where(d => d.Uid == doc.Uid && d.Rno == doc.Rno && d.No == doc.No && d.Type == type)
                .Include(d => d.Lignes)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Retourne la ligne définie par la clé et le type avec son document.
        /// </summary>
        /// <param name="keyLigne"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<LigneCLF> LigneCLFDeKey(AKeyUidRnoNo2 keyLigne, string type)
        {
            return await _context.Lignes
                .Where(d => d.Uid == keyLigne.Uid && d.Rno == keyLigne.Rno && d.No == keyLigne.No 
                && d.Uid2 == keyLigne.Uid2 && d.Rno2 == keyLigne.Rno2 && d.No2 == keyLigne.No2
                && d.Type == type)
                .Include(l => l.Doc)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Lit le dernier document du client défini par la clé et du type donné.
        /// </summary>
        /// <param name="akeyClient">clé ou Role du client</param>
        /// <param name="type">TypeCLF du document</param>
        /// <returns>le DocCLF enregistré incluant ses LigneCLF et leurs produits</returns>
        public async Task<DocCLF> DernierDoc(AKeyUidRno akeyClient, string type)
        {
            return await _context.Docs
                .Where(d => d.Uid == akeyClient.Uid && d.Rno == akeyClient.Rno && d.No > 0 && d.Type == type)
                .OrderBy(d =>  d.No)
                .Include(d => d.Lignes)
                .ThenInclude(l => l.Produit)
                .ThenInclude(p => p.Archives)
                .LastOrDefaultAsync();
        }

        public async Task<bool> EstSynthèseSansBons(DocCLF synthèse)
        {
            bool avecBons = await _context.Docs
                .Where(d => d.Uid == synthèse.Uid && d.Rno == synthèse.Rno && d.Type == TypeClf.TypeBon(synthèse.Type) && d.NoGroupe == synthèse.No)
                .AnyAsync();
            return !avecBons;
        }

        private async Task<Catalogue> Tarif(List<LigneCLF> lignes)
        {
            // 
            var produits_anciensProduits = lignes
                // Regroupe les lignes par produit et date
                .GroupBy(l => new { l.Produit, l.Date })

                // Transforme chaque groupe en un objet ayant pour membre le produit actuel
                // et l'état ancien du produit reconstitué à partir des archives ou null si le produit actuel est applicable.
                .Select(g => new
                {
                    produit = g.Key.Produit,
                    ancienProduit = g.Key.Produit.Date <= g.Key.Date
                // si la date de la dernière fin de modification de catalogue pendant laquelle le produit a été modifiée
                // est antérieure ou égale à la date passée en paramétre, le produit est utilisable dans son état actuel
                ? null
                // sinon il faut reconstituer l'état passé du produit à partir de ses archives
                : ProduitDeCatalogue.ALaDate(g.Key.Produit.Archives, g.Key.Date)
                }
                ).ToList();

            // Les données de produit nécessaires pour afficher les lignes dont les dates sont antérieures à celle
            // de la dernière modification du catalogue sont les membres ancienProduit qui ne sont pas null.
            List<ProduitDeCatalogue> produits = produits_anciensProduits
                // Filtre les objets { produit, ancien };
                .Where(p_d => p_d.ancienProduit != null)
                // Ne garde 
                .Select(p_d => p_d.ancienProduit)
                .ToList();

            // Transforme chaque objet { produit, ancienProduit } en un objet ayant pour membres le No de la catégorie
            // du produit ou de l'ancien produit et la date à laquelle il faut avoir l'état de la catégorie.
            var nosCatégorie_datesProduit = produits_anciensProduits
                .Select(p_a =>
                {
                    long noCatégorie;
                    DateTime date;
                    if (p_a.ancienProduit != null)
                    {
                        noCatégorie = p_a.ancienProduit.CategorieNo.Value;
                        date = p_a.ancienProduit.Date.Value;
                    }
                    else
                    {
                        noCatégorie = p_a.produit.CategorieNo;
                        date = p_a.produit.Date;
                    }
                    return new { noCatégorie, date };
                }).ToList();

            // Regroupe ces objets pour obtenir les no des catégories à lire dans la bdd.
            var nosCatégorie = nosCatégorie_datesProduit
                .GroupBy(no_d => no_d.noCatégorie)
                .Select(g => g.Key)
                .ToList();
            // Lit dans la bdd la liste des catégories utilisées par les produits et anciens produits.
            var catégories_ = await _context.Catégorie
                .AsNoTracking()
                .Include(c => c.Archives)
                .Where(c => nosCatégorie.Contains(c.No))
                .ToListAsync();

            // Joint ces catégories aux objets { noCatégorie, dateProduit } pour obtenir les objets { catégorie, dateProduit } correspondants.
            var catégories_dates = catégories_
                .Join(nosCatégorie_datesProduit, c => c.No, no_d => no_d.noCatégorie, (c, no_d) => new { catégorie = c, no_d.date });

            // Transforme ces objets dont la catégorie actuelle n'est pas applicable en l'état ancien de la catégorie reconstitué à partir des archives.
            List<CatégorieDeCatalogue> catégories = catégories_dates
                .Where(c_d => c_d.catégorie.Date > c_d.date)
                .Select(c_d => CatégorieDeCatalogue.ALaDate(c_d.catégorie.Archives, c_d.date))
                .ToList();


            // crée un tarif
            Catalogue tarif = new Catalogue
            {
                Produits = produits,
                Catégories = catégories
            };
            return tarif;
        }
        public async Task<Catalogue> Tarif(DocCLF doc)
        {
            return await Tarif(new List<DocCLF> { doc });
        }
        public async Task<Catalogue> Tarif(IEnumerable<DocCLF> docs)
        {
            // 
            var produits_anciensProduits = docs
                // Agrège les lignes de chaque document en un seul IEnumerable
                .Aggregate(new List<LigneCLF>(), (current, next) => current.Concat(next.Lignes).ToList())

                // Regroupe les lignes par produit et date
                .GroupBy(l => new { l.Produit, l.Date })

                // Transforme chaque groupe en un objet ayant pour membre le produit actuel, les archives de ce produit ordonnées par date
                // et l'état ancien du produit reconstitué à partir des archives ou null si le produit actuel est applicable.
                .Select(g => new
                {
                    produit = g.Key.Produit,
                    ancienProduit = g.Key.Produit.Date <= g.Key.Date
                // si la date de la dernière fin de modification de catalogue pendant laquelle le produit a été modifiée
                // est antérieure ou égale à la date passée en paramétre, le produit est utilisable dans son état actuel
                ? null
                // sinon il faut reconstituer l'état passé du produit à partir de ses archives
                : ProduitDeCatalogue.ALaDate(g.Key.Produit.Archives, g.Key.Date)
                }
                );

            // Les données de produit nécessaires pour afficher les lignes dont les dates sont antérieures à celle
            // de la dernière modification du catalogue sont les membres ancienProduit qui ne sont pas null.
            List<ProduitDeCatalogue> produits = produits_anciensProduits
                // Filtre les objets { produit, ancien };
                .Where(p_d => p_d.ancienProduit != null)
                // Ne garde 
                .Select(p_d => p_d.ancienProduit)
                .ToList();

            // Transforme chaque objet { produit, ancienProduit } en un objet ayant pour membres le No de la catégorie
            // du produit ou de l'ancien produit et la date à laquelle il faut avoir l'état de la catégorie.
            var nosCatégorie_datesProduit = produits_anciensProduits
                .Select(p_a =>
                {
                    long noCatégorie;
                    DateTime date;
                    if (p_a.ancienProduit != null)
                    {
                        noCatégorie = p_a.ancienProduit.CategorieNo.Value;
                        date = p_a.ancienProduit.Date.Value;
                    }
                    else
                    {
                        noCatégorie = p_a.produit.CategorieNo;
                        date = p_a.produit.Date;
                    }
                    return new { noCatégorie, date };
                });

            // Regroupe ces objets pour obtenir les no des catégories à lire dans la bdd.
            var nosCatégorie = nosCatégorie_datesProduit
                .GroupBy(no_d => no_d.noCatégorie)
                .Select(g => g.Key);
            // Lit dans la bdd la liste des catégories utilisées par les produits et anciens produits.
            var catégories_ = await _context.Catégorie
                .AsNoTracking()
                .Include(c => c.Archives)
                .Where(c => nosCatégorie.Contains(c.No))
                .ToListAsync();

            // Joint ces catégories aux objets { noCatégorie, dateProduit } pour obtenir les objets { catégorie, dateProduit } correspondants.
            var catégories_dates = catégories_
                .Join(nosCatégorie_datesProduit, c => c.No, no_d => no_d.noCatégorie, (c, no_d) => new { catégorie = c, no_d.date });

            // Transforme ces objets dont la catégorie actuelle n'est pas applicable en l'état ancien de la catégorie reconstitué à partir des archives.
            List<CatégorieDeCatalogue> catégories = catégories_dates
                .Where(c_d => c_d.catégorie.Date > c_d.date)
                .Select(c_d => CatégorieDeCatalogue.ALaDate(c_d.catégorie.Archives, c_d.date))
                .ToList();


            // crée un tarif
            Catalogue tarif = new Catalogue
            {
                Produits = produits,
                Catégories = catégories
            };
            return tarif;
        }

        #endregion /// Utile

        #region Lecture

        /// <summary>
        /// Retourne un CLFDocs dont le Documents contient les états de préparation des bons envoyés et sans synthèse de tous les clients.
        /// </summary>
        /// <param name="site"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<CLFDocs> ClientsAvecBons(Site site, string type)
        {
            List<DocCLF> docs = await _context.Docs
                .Where(d => d.SiteUid == site.Uid && d.SiteRno == site.Rno && d.Type == TypeClf.TypeBon(type))
                .Where(d => d.No == 0 || (d.Date != null && d.NoGroupe == null))
                .Include(d => d.Lignes)
                .AsNoTracking()
                .ToListAsync();
            return new CLFDocs
            {
                ApiDocs = docs.Select(d => CLFDoc.APréparer(d)).ToList()

            };
        }

        /// <summary>
        /// Lit la dernière commande du client.
        /// Le CLFDoc retourné contient la key du client, le No du document, les lignes sans AFixer.
        /// Si le bon de commande a été envoyé, le CLFDoc contient aussi la Date.
        /// Si le bon de commande a été inclus dans un bon de livraison enregistré, le CLFDoc contient aussi
        /// le NoGroupe et la DateGroupe et les lignes incluent leur AFixer.
        /// </summary>
        /// <param name="keyClient">key du client</param>
        /// <returns>un CLFDocs dont le champ Documents contient le CLFDoc de la dernière commande du client,
        /// CLFDoc avec </returns>
        public async Task<CLFDocs> CommandeEnCours(AKeyUidRno keyClient)
        {
            List<DocCLF> docs = await _context.Docs
                .Where(d => d.Uid == keyClient.Uid && d.Rno == keyClient.Rno && d.Type == TypeClf.Commande)
                .Where(d => d.No != 0) // exclure le bon virtuel
                .OrderByDescending(d => d.No)
                .Take(1)
                .Include(d => d.Lignes)
                .AsNoTracking()
                .ToListAsync();
            CLFDocs clfDocs = new CLFDocs
            {
                ApiDocs = docs.Select(d => CLFDoc.AvecLignes(d)).ToList()
            };
            return clfDocs;
        }

        /// <summary>
        /// Retourne un CLFDocs dont le champ ApiDocs contient un CLFDoc avec la key du client, le No du document, la Date, les lignes avec AFixer
        /// pour chacun des bons envoyés et sans synthèse d'un client et pour le bon virtuel s'il existe
        /// ou, s'il n'y en a pas et si la dernière synthèse a été créée à partir du seul bon virtuel, un CLFDoc avec la key du client, le No égal à 0,
        /// la Date de la dernière synthèse, les lignes sans AFixer correspondant aux lignes de la dernière synthèse dont le produit est toujours disponible
        /// et un NoGroupe égal au No de la dernière synthèse.
        /// S'il y a des bons non virtuels qui contiennent des lignes dont le produit ou sa catégorie ont été modifiés depuis l'enregistrement, le CLFDocs retourné
        /// contient un tarif: un catalogue contenant les produits et catégories datés appliquables à ces lignes.
        /// </summary>
        /// <param name="site"></param>
        /// <param name="keyClient"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<CLFDocs> BonsDUnClient(Site site, AKeyUidRno keyClient, string type)
        {
            string typeBon = TypeClf.TypeBon(type);
            List<DocCLF> bons = await _context.Docs
                .Where(d => d.Uid == keyClient.Uid && d.Rno == keyClient.Rno && d.Type == typeBon)
                .Where(d => d.No == 0 // bon virtuel
                    || (d.Date != null && d.NoGroupe == null)) // envoyé sans synthèse
                .Include(d => d.Lignes).ThenInclude(l => l.Produit).ThenInclude(p => p.Archives)
                .AsNoTracking()
                .ToListAsync();

            CLFDocs clfDocs = new CLFDocs
            {
                ApiDocs = new List<CLFDoc>()
            };
            if (bons.Count() > 0)
            {
                foreach (DocCLF doc in bons)
                {
                    CLFDoc cLFDoc = CLFDoc.AvecLignesAvecAFixer(doc);
                    clfDocs.ApiDocs.Add(cLFDoc);
                }
                Catalogue tarif = await Tarif(bons);
                if (tarif.Produits.Count > 0 || tarif.Catégories.Count > 0)
                {
                    clfDocs.Tarif = tarif;
                }
            }
            else
            {
                // s'il n'y a pas de bons à synthétiser et si la dernière synthèse a été créée à partir du seul bon virtuel,
                // retourne un modèle pour bon virtuel constitué à partir de la dernière synthèse en ne gardant que les lignes
                // dont le produit est toujours disponible.
                DocCLF dernièreSynthèse = await _context.Docs
                    .Where(d => d.Uid == keyClient.Uid && d.Rno == keyClient.Rno && d.Type == type)
                    .Where(d => d.Date != null)
                    .OrderBy(d => d.Date)
                    .Include(d => d.Lignes).ThenInclude(l => l.Produit)
                    .AsNoTracking()
                    .LastOrDefaultAsync();
                if (dernièreSynthèse != null && await EstSynthèseSansBons(dernièreSynthèse))
                {
                    dernièreSynthèse.Lignes = dernièreSynthèse.Lignes
                               .Where(l => l.Produit.Etat == TypeEtatProduit.Disponible)
                               .ToList();
                    // on a besoin du No de la dernière synthèse pour pouvoir afficher à l'utilisateur
                    // la source du modèle de bon virtuel
                    long noGroupe = dernièreSynthèse.No;
                    dernièreSynthèse.No = 0;
                    CLFDoc clfDoc = CLFDoc.AvecLignes(dernièreSynthèse);
                    // on fait passer dans le NoGroupe le No de la dernière synthèse pour pouvoir afficher à l'utilisateur
                    // la source du modèle de bon virtuel
                    clfDoc.NoGroupe = noGroupe;

                    clfDocs.ApiDocs.Add(clfDoc);
                }
            }
            return clfDocs;
        }

        /// <summary>
        /// Retourne la liste des documents d'un client du type demandé qui ont été envoyés (i.e. qui ont une Date) et qui ne font pas
        /// déjà partie d'une synthèse (i.e. qui n'ont pas de NoGroupe) et dont le No est dans une liste.
        /// S'il existe, le bon virtuel est considéré comme envoyé et sans synthèse.
        /// Les DocCLF retounés incluent leurs LigneCLF.
        /// Les DocCLF retournés sont "trackés" par EF Core.
        /// </summary>
        /// <param name="paramsSynthèse">a la clé du client et contient la liste des No des documents à synthétiser</param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<List<DocCLF>> DocumentsEnvoyésSansSynthèse(ParamsSynthèse paramsSynthèse, string type)
        {
            IQueryable<DocCLF> queryDocs = _context.Docs
                // Filtre client et type
                .Where(d => d.Uid == paramsSynthèse.Uid && d.Rno == paramsSynthèse.Rno && d.Type == type)
                // Filtre virtuel ou envoyés sans synthèse
                .Where(d => d.No == 0 || (d.Date != null && d.NoGroupe == null))
                // Filtre les numéros
                .Where(d => paramsSynthèse.NoDocs.Contains(d.No))
                .Include(d => d.Lignes);
            List<DocCLF> docs = await queryDocs.ToListAsync();
            return docs;
        }

        /// <summary>
        /// Retourne la liste par client des bilans (nombre et total des montants) des documents par type.
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public async Task<List<CLFClientBilanDocs>> ClientsAvecBilanDocuments(Site site)
        {
            var x = await _context.Docs
                .Where(d => d.Date != null && d.SiteUid == site.Uid && d.SiteRno == site.Rno)
                .GroupBy(d => new { d.Uid, d.Rno, d.Type })
                .Select(g => new { g.Key, Nb = g.Count(), Total = g.Sum(d => d.Total.Value), Incomplet = g.Sum(d => d.Incomplet == true ? 1 : 0) })
                .ToListAsync();
            return x
                .GroupBy(a => new { a.Key.Uid, a.Key.Rno })
                .Select(g => new CLFClientBilanDocs
                {
                    Uid = g.Key.Uid,
                    Rno = g.Key.Rno,
                    Bilans = g.Select(q =>
                    {
                        CLFBilanDocs bilan = new CLFBilanDocs
                        {
                            Type = q.Key.Type,
                            Nb = q.Nb,
                            Total = q.Total,
                        };
                        if (q.Incomplet > 0)
                        {
                            bilan.Incomplet = true;
                        }
                        return bilan;
                    }
                    ).ToList()
                })
                .ToList();
        }

        /// <summary>
        /// Retourne la liste des résumés des documents envoyés du client ou du site suivant lequel des paramètres est non null
        /// qui vérifient les conditions de type et de date définies par paramsFiltre.
        /// La liste est dans l'ordre inverse des dates et contient paramsFiltre.Nb documents si paramsFiltre.Nb est défini.
        /// </summary>
        /// <param name="paramsFiltre">définit le nombre de documents à retourner et les conditions de type et de Date</param>
        /// <param name="site"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        private async Task<List<CLFDoc>> Résumés(ParamsFiltreDoc paramsFiltre, Site site, Role client)
        {
            IQueryable<DocCLF> query = _context.Docs.Where(d => d.Date != null);
            var debug = await query.ToListAsync();
            if (site != null)
            {
                query = query.Where(d => d.SiteUid == site.Uid && d.SiteRno == site.Rno);
                debug = await query.ToListAsync();
            }
            if (client != null)
            {
                query = query.Where(d => d.Uid == client.Uid && d.Rno == client.Rno);
                debug = await query.ToListAsync();
            }
            if (paramsFiltre.Type != null)
            {
                string[] types = paramsFiltre.Type.Split(" ");
                query = query.Where(d => types.Contains(d.Type));
                debug = await query.ToListAsync();
            }
            if (paramsFiltre.DateMin != null)
            {
                query = query.Where(d => d.Date >= paramsFiltre.DateMin);
            }
            if (paramsFiltre.DateMax != null)
            {
                query = query.Where(d => d.Date.Value <= paramsFiltre.DateMax);
            }
            query = query.OrderBy(d => d.Date);
            if (paramsFiltre.I0 > 0)
            {
                query = query.Skip(paramsFiltre.I0.Value);
            }
            if (paramsFiltre.Nb > 0)
            {
                query = query.Take(paramsFiltre.Nb.Value);
            }
            List<DocCLF> docCLFs = await query.ToListAsync();
            List<CLFDoc> docs = new List<CLFDoc>();
            foreach (DocCLF docCLF in docCLFs)
            {
                docs.Add(await CLFDoc.Résumé(docCLF, _context.Docs));
            }
            return docs;
        }

        /// <summary>
        /// Retourne un CLFDocs contenant la liste des résumés des documents envoyés du site
        /// qui vérifient les conditions de type et de date définies par paramsFiltre.
        /// La liste est dans l'ordre des dates et contient paramsFiltre.Nb documents si paramsFiltre.Nb est défini.
        /// </summary>
        /// <param name="paramsFiltre">définit le nombre de documents à retourner et les conditions de type et de Date</param>
        /// <param name="site"></param>
        /// <returns></returns>
        public async Task<CLFDocs> Résumés(ParamsFiltreDoc paramsFiltre, Site site)
        {
            return new CLFDocs
            {
                ApiDocs = await Résumés(paramsFiltre, site, null)
            };
        }

        /// <summary>
        /// Retourne un CLFDocs contenant la liste des résumés des documents envoyés du client
        /// qui vérifient les conditions de type et de date définies par paramsFiltre.
        /// La liste est dans l'ordre des dates et contient paramsFiltre.Nb documents si paramsFiltre.Nb est défini.
        /// </summary>
        /// <param name="paramsFiltre">définit le nombre de documents à retourner et les conditions de type et de Date</param>
        /// <param name="client"></param>
        /// <returns></returns>
        public async Task<CLFDocs> Résumés(ParamsFiltreDoc paramsFiltre, Role client)
        {
            return new CLFDocs
            {
                ApiDocs = await Résumés(paramsFiltre, null, client)
            };
        }

        /// <summary>
        /// Date de la dernière déconnection.
        /// </summary>
        /// <param name="keyUid"></param>
        /// <returns>null si l'utilisateur ne s'est pas déconnecté avant la connection en cours</returns>
        private async Task<DateTime?> DateDernièreDéconnection(IKeyUid keyUid)
        {
            List<ArchiveUtilisateur> archives = await _context.ArchiveUtilisateur
                .Where(archive => archive.Uid == keyUid.Uid && archive.SessionId == null)
                .OrderBy(a => a.Date)
                .TakeLast(2)
                .ToListAsync();
            if (archives.Count == 1)
            {
                // c'est la première connection
                return null;
            }
            DateTime date = archives.Last().Date; // date de la connection en cours
            if (archives.First().SessionId == null)
            {
                date = archives.First().Date; // date de la dernière déconnection
                return date;
            }
            return null;
        }

        /// <summary>
        /// Retourne un CLFDocs contenant la liste des résumés des documents envoyés à l'utilisateur
        /// depuis sa dernière déconnection (bons de commande pour les sites dont l'utilisateur est fournisseur,
        /// bons de livraison et factures pour les sites dont l'utilisateur est client).
        /// La liste est dans l'ordre des dates.
        /// </summary>
        /// <param name="utilisateur">inclut les roles avec leurs site</param>
        /// <returns></returns>
        public async Task<CLFDocs> NouveauxDocs(Utilisateur utilisateur)
        {
            List<CLFDoc> nouveauxDocs = new List<CLFDoc>();
            int sessionId = utilisateur.SessionId; // > 0 car l'utilisateur est connecté
            sessionId--; // id de la session précédente
            if (sessionId != 0) // si 0, c'est la première connection
            {
                // si l'utilisateur s'est déconnecté de la session précédente, une archive
                // avec un SessionId opposé à celui de la session précédente a été enregistrée
                DateTime? dateDernièreDéconnection = await _context.ArchiveUtilisateur
                        .Where(archive => archive.Uid == utilisateur.Uid && archive.SessionId == -sessionId)
                        .Select(archive => archive.Date)
                        .FirstOrDefaultAsync();
                if (dateDernièreDéconnection != null)
                {
                    ParamsFiltreDoc paramsFiltre = new ParamsFiltreDoc
                    {
                        DateMin = dateDernièreDéconnection
                    };
                    foreach (Role role in utilisateur.Roles)
                    {
                        if (role.Uid == role.Site.Uid)
                        {
                            // c'est un role de fournisseur
                            paramsFiltre.Type = "C";
                            nouveauxDocs = nouveauxDocs.Concat(await Résumés(paramsFiltre, role.Site, null)).ToList();
                        }
                        else
                        {
                            // c'est un role de client
                            paramsFiltre.Type = "L F";
                            nouveauxDocs = nouveauxDocs.Concat(await Résumés(paramsFiltre, role.Site, null)).ToList();
                        }
                    }
                }
            }
            return new CLFDocs
            {
                ApiDocs = nouveauxDocs
            };
        }

        /// <summary>
        /// Retourne un CLFDocs qui contient le Client du document dans son état à la date du document
        /// et un ApiDocs contenant un CLFDoc avec la key, la Date, les lignes sans AFixer.
        /// Si le document fait partie d'une synthèse, le CLFDoc contient aussi le NoGroupe, la DateGroupe.
        /// Si le document est une synthèse, le CLFDoc contient aussi la liste des No des bons regroupés.
        /// S'il y a des lignes dont la date est antérieure à celle du calalogue, le CLFDocs retourné contient
        /// un tarif: un catalogue contenant les produits et catégories datés appliquables à ces lignes.
        /// </summary>
        /// <param name="site"></param>
        /// <param name="keyDocument"></param>
        /// <param name="type"></param>
        /// <returns>null si le document n'existe pas</returns>
        public async Task<CLFDocs> Document(Site site, KeyUidRnoNo keyDocument, string type)
        {
            DocCLF docCLF = await _context.Docs
                .Where(d => d.Uid == keyDocument.Uid && d.Rno == keyDocument.Rno && d.No == keyDocument.No && d.Type == type)
                .Where(d => d.Date != null)
                .Include(d => d.Lignes)
                .Include(d => d.Client).ThenInclude(c => c.Archives)
                .FirstOrDefaultAsync();
            if (docCLF == null)
            {
                return null;
            }
            CLFDoc clfDoc = CLFDoc.AvecLignes(docCLF);
            if (docCLF.NoGroupe != null)
            {
                clfDoc.NoGroupe = docCLF.NoGroupe;
                clfDoc.DateGroupe = await _context.Docs
                    .Where(d => d.Uid == docCLF.Uid && d.Rno == docCLF.Rno && d.No == docCLF.NoGroupe && d.Type == TypeClf.TypeSynthèse(docCLF.Type))
                    .Select(d => d.Date)
                    .FirstOrDefaultAsync();
            }
            if (docCLF.Type != TypeClf.Commande)
            {
                clfDoc.NoBons = await _context.Docs
                    .Where(d => d.Uid == docCLF.Uid && d.Rno == docCLF.Rno && d.NoGroupe == docCLF.No && d.Type == TypeClf.TypeBon(docCLF.Type))
                    .Select(d => d.No)
                    .ToListAsync();
            }
            RoleData client = RoleData.Ancien(docCLF.Client, docCLF.Date.Value);
            return new CLFDocs
            {
                Client = client,
                Tarif = await Tarif(docCLF),
                ApiDocs = new List<CLFDoc>
                {
                    clfDoc
                }
            };
        }

        /// <summary>
        /// Cherche un document de type livraison ou facture à partir de la key de son site, de son Type et de son No.
        /// </summary>
        /// <param name="paramsChercheDoc">key du site, no et type du document</param>
        /// <returns>un CLFChercheDoc contenant la key et le nom du client et la date si le document recherché existe, vide sinon</returns>
        public async Task<CLFChercheDoc> ChercheDocument(ParamsChercheDoc paramsChercheDoc)
        {
            DocCLF doc = await _context.Docs
                .Where(d => d.SiteUid == paramsChercheDoc.Uid && d.SiteRno == paramsChercheDoc.Rno && d.No == paramsChercheDoc.No && d.Type == paramsChercheDoc.Type)
                .Include(d => d.Client)
                .FirstOrDefaultAsync();
            if (doc == null)
            {
                return new CLFChercheDoc();
            }
            return new CLFChercheDoc
            {
                Uid = doc.Uid,
                Rno = doc.Rno,
                Nom = doc.Client.Nom,
                Date = doc.Date.Value
            };
        }

        #endregion // Lecture

        #region Action

        public async Task<RetourDeService<DocCLF>> AjouteBon(AKeyUidRno keyClient, Site site, string type, long noDoc)
        {
            DocCLF docCLF = new DocCLF
            {
                Uid = keyClient.Uid,
                Rno = keyClient.Rno,
                No = noDoc,
                SiteUid = site.Uid,
                SiteRno = site.Rno,
                Type = type
            };
            _context.Docs.Add(docCLF);
            return await SaveChangesAsync(docCLF);
        }

        /// <summary>
        /// Enregistre comme lignes d'un nouveau bon des copies des lignes d'un document précédent
        /// dont le produit est toujours disponible en mettant à jour s'il y lieu la date du catalogue applicable.
        /// </summary>
        /// <param name="bon">nouveau bon auquel on veut ajouter des lignes</param>
        /// <param name="docACopier">document incluant ses lignes</param>
        /// <param name="dateCatalogue">date du catalogue du site</param>
        /// <returns></returns>
        public async Task<RetourDeService> CopieLignes(DocCLF bon, DocCLF docACopier, DateTime dateCatalogue)
        {
            Action<LigneCLF, LigneCLF> copieQuantité;
            if (bon.No == 0)
            {
                copieQuantité = (LigneCLF ligne, LigneCLF copie) =>
                {
                    copie.AFixer = ligne.Quantité;
                };
            }
            else
            {
                copieQuantité = (LigneCLF ligne, LigneCLF copie) =>
                {
                    copie.Quantité = ligne.Quantité;
                };
            }
            LigneCLF copieLigne(LigneCLF ligne)
            {
                LigneCLF copie = new LigneCLF
                {
                    Type = bon.Type,
                    TypeCommande = ligne.TypeCommande,
                };
                copie.CopieKey(ligne);
                copie.Date = dateCatalogue;
                copie.No = bon.No;
                copieQuantité(ligne, copie);
                return copie;
            }
            IEnumerable<LigneCLF> lignesCopiées = docACopier.Lignes
                .Where(ligne => ligne.Produit.Etat == TypeEtatProduit.Disponible)
                .Select(ligne => copieLigne(ligne));
            _context.Lignes.AddRange(lignesCopiées);
            return await SaveChangesAsync();
        }

        /// <summary>
        /// Efface toutes les lignes du bon et si le bon est virtuel, supprime le bon.
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public async Task<RetourDeService> EffaceBonEtSupprimeSiVirtuel(DocCLF doc)
        {
            _context.Lignes.RemoveRange(doc.Lignes);
            if (doc.No == 0)
            {
                _context.Docs.Remove(doc);
            }
            return await SaveChangesAsync();
        }

        /// <summary>
        /// Fixe la date et les champs de résumé du DocCLF. Fixe la date des lignes à celle du catalogue. Sauvegarde.
        /// </summary>
        /// <param name="docCLF"></param>
        /// <returns>un CLFDOc contenant uniquement la date</returns>
        public async Task<RetourDeService<CLFDoc>> EnvoiCommande(Site site, DocCLF docCLF)
        {
            docCLF.Date = DateTime.Now;
            docCLF.NbLignes = docCLF.Lignes.Count;
            docCLF.Total = 0;
            KeyUidRno keySite = new KeyUidRno
            {
                Uid = docCLF.SiteUid,
                Rno = docCLF.SiteRno
            };
            docCLF.Lignes.ToList().ForEach((LigneCLF ligne) =>
            {
                if (ligne.TypeCommande == TypeUnitéDeCommande.Unité && ligne.Produit.TypeCommande==TypeUnitéDeCommande.UnitéOuVrac)
                {
                    docCLF.Incomplet = true;
                }
                else
                {
                    docCLF.Total += ligne.Quantité * ligne.Produit.Prix;
                }
            });
            CLFDoc docRetour = CLFDoc.DeDate(docCLF);
            _context.Docs.Update(docCLF);
            RetourDeService<CLFDoc> retour = await SaveChangesAsync(docRetour);
            List<LigneCLF> lignesDatées = docCLF.Lignes.Select(l => LigneCLF.Clone(site.DateCatalogue.Value, l)).ToList();
           _context.Lignes.RemoveRange(docCLF.Lignes);
            retour = await SaveChangesAsync(docRetour);
            _context.Lignes.AddRange(lignesDatées);
            retour = await SaveChangesAsync(docRetour);
            return retour;
        }

        public async Task<RetourDeService> AjouteLigneCommande(Site site, CLFLigne ligne)
        {
            LigneCLF ajout = new LigneCLF
            {
                Quantité = ligne.Quantité,
                AFixer = ligne.AFixer,
                Type = TypeClf.Commande,
                TypeCommande = ligne.TypeCommande,
                Date = site.DateCatalogue.Value
            };
            ajout.CopieKey(ligne);
            _context.Lignes.Add(ajout);
            return await SaveChangesAsync();
        }

        public async Task<RetourDeService> EditeLigne(LigneCLF ligne, CLFLigne lignePostée)
        {
            ligne.Quantité = lignePostée.Quantité;
            ligne.AFixer = lignePostée.AFixer;
            ligne.TypeCommande = lignePostée.TypeCommande;
            _context.Lignes.Update(ligne);
            return await SaveChangesAsync();
        }

        public async Task<RetourDeService<LigneCLF>> FixeLigne(LigneCLF ligne, decimal àFixer)
        {
            ligne.AFixer = àFixer;
            _context.Lignes.Update(ligne);
            return await SaveChangesAsync(ligne);
        }

        public async Task<RetourDeService> SupprimeLigne(LigneCLF ligne)
        {
            _context.Lignes.Remove(ligne);
            return await SaveChangesAsync();
        }

        /// <summary>
        /// Copie si possible la valeur de Quantité dans AFixer pour chaque ligne
        /// La copie est impossible si Quantité n'est pas défini ou si la ligne est dans une commande
        /// et demande un nombre de pièces d'un produit dont le prix dépend d'une mesure.
        /// </summary>
        /// <param name="iqLignes"></param>
        /// <param name="type"></param>
        /// <returns>null s'il n y pas de lignes où la copie est possible</returns>
        private async Task<RetourDeService> CopieQuantité(IQueryable<LigneCLF> iqLignes, string type)
        {
            if (type == TypeClf.Commande)
            {
                iqLignes = iqLignes
                    .Include(l => l.Produit)
                    .Where(l => !(l.Produit.TypeCommande == TypeUnitéDeCommande.UnitéOuVrac && l.TypeCommande == TypeUnitéDeCommande.Vrac));
            }
            List<LigneCLF> copiables = await iqLignes.ToListAsync();
            if (copiables.Count == 0)
            {
                return null;
            }
            copiables.ForEach(l => l.AFixer = l.Quantité);
            _context.Lignes.UpdateRange(copiables);
            return await SaveChangesAsync();
        }

        /// <summary>
        /// Copie si possible la valeur de Quantité dans AFixer pour la ligne définie par la key et le type.
        /// La copie est impossible si Quantité n'est pas défini ou si la ligne est dans une commande
        /// et demande un nombre de pièces d'un produit dont le prix dépend d'une mesure.
        /// </summary>
        /// <param name="keyLigne"></param>
        /// <param name="type"></param>
        /// <returns>null si la copie est impossible</returns>
        public async Task<RetourDeService> CopieQuantité(AKeyUidRnoNo2 keyLigne, string type)
        {
            IQueryable<LigneCLF> iqLignes = _context.Lignes
               .Where(l => l.Type == type
                    && l.Uid == keyLigne.Uid && l.Rno == keyLigne.Rno && l.No == keyLigne.No
                    && l.Uid2 == keyLigne.Uid2 && l.Rno2 == keyLigne.Rno2 && l.No2 == keyLigne.No2
                    && l.Date == keyLigne.Date);
            return await CopieQuantité(iqLignes, type);
        }

        /// <summary>
        /// Copie si possible la valeur de Quantité dans AFixer pour chaque ligne du document défini par la key et le type.
        /// La copie est impossible si Quantité n'est pas défini ou si la ligne est dans une commande
        /// et demande un nombre de pièces d'un produit dont le prix dépend d'une mesure.
        /// </summary>
        /// <param name="keyDoc"></param>
        /// <param name="type"></param>
        /// <returns>null s'il n y pas de lignes où la copie est possible</returns>
        public async Task<RetourDeService> CopieQuantité(AKeyUidRnoNo keyDoc, string type)
        {
            IQueryable<LigneCLF> iqLignes = _context.Lignes
               .Where(l => l.Type == type
                    && l.Uid == keyDoc.Uid && l.Rno == keyDoc.Rno && l.No == keyDoc.No);
            return await CopieQuantité(iqLignes, type);
        }

        /// <summary>
        /// Copie si possible la valeur de Quantité dans AFixer pour chaque ligne des documents de la liste.
        /// La copie est impossible si Quantité n'est pas défini ou si la ligne est dans une commande
        /// et demande un nombre de pièces d'un produit dont le prix dépend d'une mesure.
        /// </summary>
        /// <param name="docs"></param>
        /// <param name="type"></param>
        /// <returns>null s'il n y pas de lignes où la copie est possible</returns>
        public async Task<RetourDeService> CopieQuantité(List<DocCLF> docs, string type)
        {
            List<LigneCLF> lignes = new List<LigneCLF>();
            docs.ForEach(d => lignes.AddRange(d.Lignes));
            if (type == TypeClf.Livraison)
            {
                lignes = lignes
                    .Where(l => 
                    !(l.Produit.TypeCommande == TypeUnitéDeCommande.UnitéOuVrac && l.TypeCommande == TypeUnitéDeCommande.Vrac))
                    .ToList();
            }
            lignes.ForEach(l => l.AFixer = l.Quantité);
            _context.Lignes.UpdateRange(lignes);
            return await SaveChangesAsync();
        }

        /// <summary>
        /// Annule la valeur de AFixer pour chaque ligne du type
        /// </summary>
        /// <param name="iqLignes">IQueryable des lignes à annuler</param>
        /// <param name="type"></param>
        /// <returns>null s'il n y pas de lignes où la copie est possible</returns>
        private async Task<RetourDeService> Annule(IQueryable<LigneCLF> iqLignes, string type)
        {
            List<LigneCLF> lignes = await iqLignes
                .Where(l => l.Type == type)
                .ToListAsync();
            lignes.ForEach(l => l.AFixer = 0);
            _context.Lignes.UpdateRange(lignes);
            return await SaveChangesAsync();
        }

        /// <summary>
        /// Annule la valeur de AFixer pour la ligne définie par la key et le type si AFixer n'est pas défini.
        /// </summary>
        /// <param name="keyLigne"></param>
        /// <param name="type"></param>
        /// <returns>null si la copie est impossible</returns>
        public async Task<RetourDeService> Annule(AKeyUidRnoNo2 keyLigne, string type)
        {
            IQueryable<LigneCLF> iqLignes = _context.Lignes
                .Where(l => l.Uid == keyLigne.Uid && l.Rno == keyLigne.Rno && l.No == keyLigne.No
                && l.Uid2 == keyLigne.Uid2 && l.Rno2 == keyLigne.Rno2 && l.No2 == keyLigne.No2
                && l.Date == keyLigne.Date);
            return await Annule(iqLignes, type);
        }

        /// <summary>
        /// Annule la valeur de AFixer pour chaque ligne du document défini par la key et le type
        /// </summary>
        /// <param name="keyDoc"></param>
        /// <param name="type"></param>
        /// <returns>null s'il n y pas de lignes où la copie est possible</returns>
        public async Task<RetourDeService> Annule(AKeyUidRnoNo keyDoc, string type)
        {
            IQueryable<LigneCLF> iqLignes = _context.Lignes
                .Where(l => l.Uid == keyDoc.Uid && l.Rno == keyDoc.Rno && l.No == keyDoc.No);
            return await Annule(iqLignes, type);
        }

        /// <summary>
        /// Annule la valeur de AFixer pour chaque ligne des documents de la liste.
        /// </summary>
        /// <param name="docs"></param>
        /// <param name="type"></param>
        /// <returns>null s'il n y pas de lignes où la copie est possible</returns>
        public async Task<RetourDeService> Annule(List<DocCLF> docs, string type)
        {
            List<LigneCLF> lignes = new List<LigneCLF>();
            docs.ForEach(d => lignes.AddRange(d.Lignes));
            lignes.ForEach(l => l.AFixer = 0);
            _context.Lignes.UpdateRange(lignes);
            return await SaveChangesAsync();
        }

        /// <summary>
        /// Crée un document de synthèse à partir des documents de la liste. Fixe le NoGroupe des documents de la liste.
        /// Si le bon virtuel figure dans la liste, supprime le bon virtuel.
        /// L'objet retourné contient un DocCLF contenant uniquement le No et la Date de la synthèse créée.
        /// </summary>
        /// <param name="site"></param>
        /// <param name="client"></param>
        /// <param name="docCLFs">les documents à synthétiser incluent leur lignes et ne sont pas traçés par EF Core</param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<RetourDeService<DocCLF>> Synthèse(Site site, Role client, List<DocCLF> docCLFs, string type)
        {
            // Les No des synthèses d'un type se suivent par site.
            // Dernière synthèse de ce type ou null s'il n'y a jamais eu de synthèse de ce type.
            DocCLF dernier = await _context.Docs
                .Where(d => d.SiteUid == site.Uid && d.SiteRno == site.Rno && d.No > 0 && d.Type == type)
                .OrderBy(d => d.No)
                .AsNoTracking()
                .LastOrDefaultAsync();
            DocCLF synthèse = new DocCLF
            {
                Uid = client.Uid,
                Rno = client.Rno,
                No = dernier == null ? 1 : dernier.No + 1,
                SiteUid = site.Uid,
                SiteRno = site.Rno,
                Type = type,
                Date = DateTime.Now
            };

            DocCLF virtuel = docCLFs
                .Where(d => d.No == 0)
                .FirstOrDefault();
            if (virtuel != null)
            {
                foreach (LigneCLF ligne in virtuel.Lignes)
                {
                    ligne.Date = site.DateCatalogue.Value;
                }
            }

            IEnumerable<ProduitDeCatalogue> anciensProduits = (await Tarif(docCLFs)).Produits.Select(p => p);
                
            Tuple<List<LigneCLF>, decimal> lignes_coût = docCLFs
                // fixe le NoGroupe de chaque bon
                .Select(d => { d.NoGroupe = synthèse.No; return d; })
                // remplace les bons par leurs listes de Tuples <dateCatalogue, lignes>
                .Select(d => d.Lignes.Select(l => Tuple.Create(l.Date, l)).ToList())
                // agrége les Tuples <dateCatalogue, ligne> des bons en une seule liste
                .Aggregate(new List<Tuple<DateTime, LigneCLF>>(), (current, next) => current.Concat(next).ToList())
                // remplace les Tuples <dateCatalogue, ligne> par des objets { dateCatalogue, ligne, noProduit }
                .Select(t => new { date = t.Item1, ligne = t.Item2, noProduit = t.Item2.No2 })
                // groupe  les objets { dateCatalogue, ligne, noProduit } ayant mêmes dateCatalogue et noProduit
                .GroupBy(t => new { t.date, t.noProduit })
                // remplace les groupes par des objets { produit, ancien_produit, lignes }
                .Select(g => new
                {
                    produit = g.First().ligne.Produit,
                    ancien_produit = anciensProduits.Where(p => p.No == g.Key.noProduit && p.Date == g.Key.date).FirstOrDefault(),
                    // toutes les lignes du groupe ont le même produit
                    lignes = g.Select(l => l.ligne)
                })

                .Select(o => new
                {
                    noProduit = o.produit.No,
                    date = o.ancien_produit == null ? o.produit.Date : o.ancien_produit.Date,
                    prix = o.ancien_produit == null ? o.produit.Prix : o.ancien_produit.Prix,
                    aFixer = o.lignes
                        .Select(l => l.AFixer.Value)
                        .Aggregate((decimal)0, (valeur, suivant) => valeur + suivant)
                })
                .Select(o => Tuple.Create(
                    new LigneCLF
                    {
                        Uid = synthèse.Uid,
                        Rno = synthèse.Rno,
                        No = synthèse.No,
                        Uid2 = site.Uid,
                        Rno2 = site.Rno,
                        No2 = o.noProduit,
                        Type = type,
                        Quantité = o.aFixer,
                        Date = o.date.Value
                    },
                    o.prix.Value * o.aFixer
                ))
                .Aggregate(Tuple.Create(new List<LigneCLF>(), (decimal) 0),
                (valeur, suivant) => { valeur.Item1.Add(suivant.Item1); return Tuple.Create(valeur.Item1, valeur.Item2 + suivant.Item2); });

            synthèse.NbLignes = lignes_coût.Item1.Count();
            synthèse.Total = lignes_coût.Item2;

            _context.Docs.Add(synthèse);
            DocCLF bonVirtuel = docCLFs.Where(d => d.No == 0).FirstOrDefault();
            if (bonVirtuel != null)
            {
                _context.Docs.Remove(bonVirtuel);
                docCLFs.Remove(bonVirtuel);
            }
            RetourDeService<DocCLF> retour = await SaveChangesAsync(synthèse);

            if (retour.Ok)
            {
                _context.Lignes.AddRange(lignes_coût.Item1);
                _context.Docs.UpdateRange(docCLFs);
                DocCLF docRetour = new DocCLF
                {
                    No = synthèse.No,
                    Date = synthèse.Date
                };
                return await SaveChangesAsync(docRetour);
            }

            return retour;
        }

        #endregion

    }
}
