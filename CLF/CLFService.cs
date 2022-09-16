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
        /// <param name="keyDdoc">objet ayant l'Id et le No du document cherché</param>
        /// <param name="type">TypeCLF du document cherché</param>
        /// <returns>le DocCLF incluant ses Lignes, si trouvé; null, sinon.</returns>
        public async Task<DocCLF> DocCLFDeKey(IKeyDocSansType keyDdoc, TypeCLF type)
        {
            return await _context.Doc
                .Where(d => d.Id == keyDdoc.Id && d.No == keyDdoc.No && d.Type == type)
                .Include(d => d.Lignes)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Retourne la ligne définie par la clé et le type avec son document.
        /// </summary>
        /// <param name="keyLigne"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<LigneCLF> LigneCLFDeKey(IKeyLigneSansType keyLigne, TypeCLF type)
        {
            return await _context.Ligne
                .Where(d => d.Id == keyLigne.Id && d.No == keyLigne.No 
                && d.ProduitId == keyLigne.ProduitId
                && d.Type == type)
                .Include(l => l.Doc)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Lit le dernier document du client défini par la clé et du type donné.
        /// </summary>
        /// <param name="idClient">clé ou Role du client</param>
        /// <param name="type">TypeCLF du document</param>
        /// <returns>le DocCLF enregistré incluant ses LigneCLF et leurs produits</returns>
        public async Task<DocCLF> DernierDoc(uint idClient, TypeCLF type)
        {
            return await _context.Doc
                .Where(d => d.Id == idClient && d.No > 0 && d.Type == type)
                .OrderBy(d =>  d.No)
                .Include(d => d.Lignes)
                .ThenInclude(l => l.Produit)
                .ThenInclude(p => p.Archives)
                .LastOrDefaultAsync();
        }

        public async Task<bool> EstSynthèseSansBons(DocCLF synthèse)
        {
            bool avecBons = await _context.Doc
                .Where(d => d.Id == synthèse.Id && d.Type == DocCLF.TypeBon(synthèse.Type) && d.NoGroupe == synthèse.No)
                .AnyAsync();
            return !avecBons;
        }

        private async Task<Catalogue> Tarif(IEnumerable<LigneCLF> lignes)
        {
            // Lignes groupées par ProduitId et Date
            var lignesParProduitIdEtDate = lignes
                .GroupBy(l => new { l.ProduitId, l.Date });
            // Ids des Produits utilisés dans les lignes
            IEnumerable<uint> produitIds = lignesParProduitIdEtDate
                .GroupBy(g => g.Key.ProduitId)
                .Select(g1 => g1.Key);
            // Produits utilisés dans les lignes incluant leurs archives et leurs catégories incluant leurs archives
            List<Produit> produitsUtilisés = await _context.Produit
                .AsNoTracking()
                .Where(p => produitIds.Contains(p.Id))
                .Include(p => p.Archives)
                .ToListAsync();
            // Produits utilisés dans les lignes
            var produitsAEnvoyer_dates = lignesParProduitIdEtDate
                .Join(produitsUtilisés,
                g => g.Key.ProduitId,
                p => p.Id,
                (g, p) =>
                {
                    bool dansTarif = DateTime.Compare(p.Date, g.Key.Date) == 1;
                    return new
                    {
                        dansTarif,
                        produit = dansTarif ? ProduitAEnvoyer.ALaDate(p, g.Key.Date) : ProduitAEnvoyer.SansEtatNiDate(p),
                        date = g.Key.Date,
                    };
                }).ToList();
            List<ProduitAEnvoyer> produits = produitsAEnvoyer_dates
                .Where(p_d => p_d.dansTarif)
                .Select(p_d => p_d.produit)
                .ToList();

            // Ids des Catégories des Produits utilisés dans les lignes
            IEnumerable<uint> catégoriesIds = produitsAEnvoyer_dates
                .GroupBy(p_d => p_d.produit.CategorieId.Value)
                .Select(g1 => g1.Key);
            // Catégories des Produits utilisés dans les lignes incluant leurs archives
            List<Catégorie> catégoriesUtilisées = await _context.Catégorie
                .AsNoTracking()
                .Where(c => catégoriesIds.Contains(c.Id))
                .Include(c => c.Archives)
                .ToListAsync();
            List<CatégorieAEnvoyer> catégories = produitsAEnvoyer_dates
                .Join(catégoriesUtilisées,
                p_d => p_d.produit.CategorieId.Value,
                c => c.Id,
                (p_d, c) =>
                {
                    bool dansTarif = DateTime.Compare(c.Date, p_d.date) == 1;
                    return dansTarif ? CatégorieAEnvoyer.ALaDate(c, p_d.date) : null;
                })
                .Where(c => c != null)
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
            return await Tarif(doc.Lignes);
        }
        public async Task<Catalogue> Tarif(IEnumerable<DocCLF> bons)
        {
            List<LigneCLF> lignes = bons.Aggregate(new List<LigneCLF>(), (current, next) => current.Concat(next.Lignes).ToList());
            return await Tarif(lignes);
        }

        /// <summary>
        /// Recherche un Client à partir de son Id.
        /// </summary>
        /// <param name="idClient">Id du client recherché</param>
        /// <returns>un Client incluant son Site avec son Fournisseur, si trouvé; null, sinon.</returns>
        public async Task<Client> ClientAvecSite(uint idClient)
        {
            Client client = await _context.Client
                .Where(cl => cl.Id == idClient)
                .Include(cl => cl.Site).ThenInclude(s => s.Fournisseur)
                .AsNoTracking()
                .FirstOrDefaultAsync();
            return client;
        }

        #endregion /// Utile

        #region Lecture

        /// <summary>
        /// Retourne un CLFDocs dont le Documents contient les états de préparation des bons envoyés et sans synthèse de tous les clients.
        /// </summary>
        /// <param name="idSite">Id du Site</param>
        /// <param name="type">TypeCLF des documents</param>
        /// <returns></returns>
        public async Task<CLFDocs> ClientsAvecBons(uint idSite, TypeCLF type)
        {
            List<DocCLF> docs = await _context.Doc
                .Include(d=>d.Client)
                .Where(d => d.Client.SiteId == idSite && d.Type == DocCLF.TypeBon(type))
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
        /// <param name="idClient">Id du client</param>
        /// <returns>un CLFDocs dont le champ Documents contient le CLFDoc de la dernière commande du client,
        /// CLFDoc avec </returns>
        public async Task<CLFDocs> CommandeEnCours(uint idClient)
        {
            DocCLF docCLF = await _context.Doc
                .Where(d => d.Id == idClient && d.Type == TypeCLF.Commande)
                .Where(d => d.No != 0) // exclure le bon virtuel
                .OrderByDescending(d => d.No)
                .Take(1)
                .Include(d => d.Lignes).ThenInclude(l => l.Produit)
                .FirstOrDefaultAsync();
            if (docCLF == null)
            {
                return new CLFDocs
                {
                    ApiDocs = new List<CLFDoc>()
                };
            }
            CLFDoc cLFDoc = CLFDoc.DeNo(docCLF.No);
            cLFDoc.Id = idClient;
            List<LigneCLF> disponibles = docCLF.Lignes.Where(l => l.Produit.Disponible).ToList();
            List<LigneCLF> indisponibles = docCLF.Lignes.Where(l => !l.Produit.Disponible).ToList();
            if (docCLF.Date == null)
            {
                // le bon de commande n'a pas été envoyé
                // il faut supprimer de la bdd les lignes dont le produit est devenu indisponible
                if (indisponibles.Count > 0)
                {
                    _context.Ligne.RemoveRange(indisponibles);
                    await SaveChangesAsync();
                }
            }
            else
            {
                cLFDoc.Date = docCLF.Date;
                if (docCLF.NoGroupe != null)
                {
                    // le bon de commande a été inclus dans un bon de livraison enregistré
                    DocCLF livraison = await _context.Doc
                        .Where(d => d.Id == idClient && d.No == docCLF.NoGroupe && d.Type == TypeCLF.Livraison)
                        .AsNoTracking()
                        .FirstOrDefaultAsync();
                    cLFDoc.NoGroupe = docCLF.NoGroupe;
                    cLFDoc.DateGroupe = livraison.Date;
                }
            }
            // on n'envoie que les lignes dont le produit est toujours disponible
            cLFDoc.Lignes = disponibles.Select(l => CLFLigneData.LigneData(l)).ToList();
            CLFDocs clfDocs = new CLFDocs
            {
                ApiDocs = new List<CLFDoc> { cLFDoc }
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
        /// <param name="idClient"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<CLFDocs> BonsDUnClient(Site site, uint idClient, TypeCLF type)
        {
            TypeCLF typeBon = DocCLF.TypeBon(type);
            List<DocCLF> bons = await _context.Doc
                .Where(d => d.Id == idClient && d.Type == typeBon)
                .Where(d => d.No == 0 // bon virtuel
                    || (d.Date != null && d.NoGroupe == null)) // envoyé sans synthèse
                .Include(d => d.Lignes)
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
                DocCLF dernièreSynthèse = await _context.Doc
                    .Where(d => d.Id == idClient && d.Type == type)
                    .Where(d => d.Date != null)
                    .OrderBy(d => d.Date)
                    .Include(d => d.Lignes).ThenInclude(l => l.Produit)
                    .AsNoTracking()
                    .LastOrDefaultAsync();
                if (dernièreSynthèse != null && await EstSynthèseSansBons(dernièreSynthèse))
                {
                    dernièreSynthèse.Lignes = dernièreSynthèse.Lignes
                               .Where(l => l.Produit.Disponible)
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
        /// <param name="type">TypeCLF du document</param>
        /// <returns></returns>
        public async Task<List<DocCLF>> DocumentsEnvoyésSansSynthèse(ParamsSynthèse paramsSynthèse, TypeCLF type)
        {
            IQueryable<DocCLF> queryDocs = _context.Doc
                // Filtre client et type
                .Where(d => d.Id == paramsSynthèse.Id && d.Type == type)
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
            var x = await _context.Doc
                .Include(d => d.Client)
                .Where(d => d.Date != null && d.Client.SiteId == site.Id)
                .GroupBy(d => new { d.Id, d.Type })
                .Select(g => new { g.Key, Nb = g.Count(), Total = g.Sum(d => d.Total.Value), Incomplet = g.Sum(d => d.Incomplet == true ? 1 : 0) })
                .ToListAsync();
            return x
                .GroupBy(a => new { a.Key.Id })
                .Select(g => new CLFClientBilanDocs
                {
                    Id = g.Key.Id,
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
        /// <param name="idSite">Id du Site</param>
        /// <param name="idClient">Id du Client</param>
        /// <returns></returns>
        private async Task<List<CLFDoc>> Résumés(ParamsFiltreDoc paramsFiltre, uint? idSite, uint? idClient)
        {
            IQueryable<DocCLF> query = _context.Doc.Where(d => d.Date != null).Include(d => d.Client);
            var debug = await query.ToListAsync();
            if (idSite.HasValue)
            {
                query = query.Where(d => d.Client.SiteId == idSite.Value);
                debug = await query.ToListAsync();
            }
            if (idClient != null)
            {
                query = query.Where(d => d.Id == idClient);
                debug = await query.ToListAsync();
            }
            if (paramsFiltre.Type.HasValue)
            {
                query = query.Where(d => d.Type == paramsFiltre.Type.Value);
                debug = await query.ToListAsync();
            }
            else
            {
                if (paramsFiltre.Type1Ou2 != null)
                {
                    query = query.Where(d => d.Type == paramsFiltre.Type1Ou2[0] || d.Type == paramsFiltre.Type1Ou2[1]);
                    debug = await query.ToListAsync();
                }
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
                docs.Add(await CLFDoc.Résumé(docCLF, _context.Doc));
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
                ApiDocs = await Résumés(paramsFiltre, site.Id, null)
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
        public async Task<CLFDocs> Résumés(ParamsFiltreDoc paramsFiltre, Client client)
        {
            return new CLFDocs
            {
                ApiDocs = await Résumés(paramsFiltre, null, client.Id)
            };
        }

        /// <summary>
        /// Retourne un CLFDocs contenant la liste des résumés des documents envoyés à l'utilisateur
        /// depuis sa dernière déconnection (bons de commande pour les sites dont l'utilisateur est fournisseur,
        /// bons de livraison et factures pour les sites dont l'utilisateur est client).
        /// La liste est dans l'ordre des dates.
        /// </summary>
        /// <param name="utilisateur">Utilisateur qui inclut ses Fournisseurs avec leur site et ses Clients</param>
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
                        .Where(archive => archive.Id == utilisateur.Id && archive.SessionId == -sessionId)
                        .Select(archive => archive.Date)
                        .FirstOrDefaultAsync();
                if (dateDernièreDéconnection != null)
                {
                    ParamsFiltreDoc paramsFiltre = new ParamsFiltreDoc
                    {
                        DateMin = dateDernièreDéconnection
                    };
                    foreach (Fournisseur fournisseur in utilisateur.Fournisseurs)
                    {
                        paramsFiltre.Type = TypeCLF.Commande;
                        nouveauxDocs = nouveauxDocs.Concat(await Résumés(paramsFiltre, fournisseur.Id, null)).ToList();
                    }
                    foreach (Client client in utilisateur.Clients)
                    {
                        paramsFiltre.Type1Ou2 = new TypeCLF[] { TypeCLF.Livraison, TypeCLF.Facture };
                            nouveauxDocs = nouveauxDocs.Concat(await Résumés(paramsFiltre, client.SiteId, null)).ToList();
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
        /// <param name="keyDocument"></param>
        /// <param name="type"></param>
        /// <returns>null si le document n'existe pas</returns>
        public async Task<CLFDocs> Document(KeyDocSansType keyDocument, TypeCLF type)
        {
            DocCLF docCLF = await _context.Doc
                .Where(d => d.Id == keyDocument.Id && d.No == keyDocument.No && d.Type == type)
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
                TypeCLF typeSynthèse = DocCLF.TypeSynthèse(docCLF.Type);
                clfDoc.DateGroupe = await _context.Doc
                    .Where(d => d.Id == docCLF.Id && d.No == docCLF.NoGroupe && d.Type == typeSynthèse)
                    .Select(d => d.Date)
                    .FirstOrDefaultAsync();
            }
            if (docCLF.Type != TypeCLF.Commande)
            {
                TypeCLF typeBon = DocCLF.TypeBon(docCLF.Type);
                clfDoc.NoBons = await _context.Doc
                    .Where(d => d.Id == docCLF.Id && d.NoGroupe == docCLF.No && d.Type == typeBon)
                    .Select(d => d.No)
                    .ToListAsync();
            }

            // Reconstitue le Client à la Date du document
            IOrderedEnumerable<ArchiveClient> archives = docCLF.Client.Archives.OrderBy(a => a.Date);
            ClientData client = null;
            if (archives.Last().Date > docCLF.Date.Value)
            {
                client = new ClientData();
                archives.ToList().ForEach(archive =>
                {
                    Client.CopieDataSiPasNull(archive, client);
                });
            }
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
        /// <param name="paramsChercheDoc">key du site, id et type du document</param>
        /// <returns>un CLFDoc contenant la key et le nom du client et la date si le document recherché existe, null sinon</returns>
        public async Task<CLFDoc> ChercheDocument(ParamsChercheDoc paramsChercheDoc)
        {
            DocCLF doc = await _context.Doc
                .Include(d => d.Client)
                .Where(d => d.Client.SiteId == paramsChercheDoc.Id && d.No == paramsChercheDoc.No && d.Type == paramsChercheDoc.Type)
                .FirstOrDefaultAsync();
            if (doc == null)
            {
                return null;
            }
            return CLFDoc.DeIdNomNoDate(doc);
        }

        #endregion // Lecture

        #region Action

        public async Task<RetourDeService<DocCLF>> AjouteBon(uint idClient, TypeCLF type, uint noDoc)
        {
            DocCLF docCLF = new DocCLF
            {
                Id = idClient,
                No = noDoc,
                Type = type
            };
            _context.Doc.Add(docCLF);
            return await SaveChangesAsync(docCLF);
        }

        /// <summary>
        /// Enregistre comme lignes d'un nouveau bon des copies des lignes d'un document précédent
        /// dont le produit est toujours disponible en mettant à jour s'il y lieu la date du catalogue applicable.
        /// </summary>
        /// <param name="bon">nouveau bon auquel on veut ajouter des lignes</param>
        /// <param name="docACopier">document incluant ses lignes</param>
        /// <returns></returns>
        public async Task<RetourDeService> CopieLignes(DocCLF bon, DocCLF docACopier)
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
                    Id = bon.Id,
                    No = bon.No,
                    Type = bon.Type,
                    ProduitId = ligne.ProduitId,
                };
                copie.Date = ligne.Produit.Date;
                copie.No = bon.No;
                copieQuantité(ligne, copie);
                return copie;
            }
            IEnumerable<LigneCLF> lignesCopiées = docACopier.Lignes
                .Where(ligne => ligne.Produit.Disponible)
                .Select(ligne => copieLigne(ligne));
            _context.Ligne.AddRange(lignesCopiées);
            return await SaveChangesAsync();
        }

        /// <summary>
        /// Efface toutes les lignes du bon et si le bon est virtuel, supprime le bon.
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public async Task<RetourDeService> EffaceBonEtSupprimeSiVirtuel(DocCLF doc)
        {
            _context.Ligne.RemoveRange(doc.Lignes);
            if (doc.No == 0)
            {
                _context.Doc.Remove(doc);
            }
            return await SaveChangesAsync();
        }

        /// <summary>
        /// Fixe la date et les champs de résumé du DocCLF. Fixe les dates des lignes à celles de leur Produit. Sauvegarde.
        /// </summary>
        /// <param name="docCLF">DocCLF incluant ses lignes incluant leur Produit</param>
        /// <returns>un CLFDOc contenant uniquement la date</returns>
        public async Task<RetourDeService<CLFDoc>> EnvoiCommande(Site site, DocCLF docCLF)
        {
            docCLF.Date = DateTime.Now;
            docCLF.NbLignes = docCLF.Lignes.Count;
            docCLF.Total = 0;
            docCLF.Lignes.ToList().ForEach((LigneCLF ligne) =>
            {
                if (ligne.Produit.SCALP == true)
                {
                    docCLF.Incomplet = true;
                }
                else
                {
                    docCLF.Total += ligne.Quantité * ligne.Produit.Prix;
                }
                ligne.Date = ligne.Produit.Date;
            });
            CLFDoc docRetour = CLFDoc.DeDate(docCLF);
            _context.Doc.Update(docCLF);
            _context.Ligne.UpdateRange(docCLF.Lignes);
            RetourDeService<CLFDoc> retour = await SaveChangesAsync(docRetour);
            return retour;
            List<LigneCLF> lignesDatées = docCLF.Lignes.Select(l => LigneCLF.Clone(site.DateCatalogue.Value, l)).ToList();
           _context.Ligne.RemoveRange(docCLF.Lignes);
            retour = await SaveChangesAsync(docRetour);
            _context.Ligne.AddRange(lignesDatées);
            retour = await SaveChangesAsync(docRetour);
        }

        public async Task<RetourDeService> AjouteLigneCommande(Produit produit, CLFLigne ligne)
        {
            LigneCLF ajout = new LigneCLF
            {
                Id = ligne.Id,
                No = ligne.No,
                Quantité = ligne.Quantité,
                AFixer = ligne.AFixer,
                Type = TypeCLF.Commande,
                ProduitId = ligne.ProduitId,
                Date = produit.Date
            };
            _context.Ligne.Add(ajout);
            return await SaveChangesAsync();
        }

        public async Task<RetourDeService> EditeLigne(LigneCLF ligne, CLFLigne lignePostée)
        {
            ligne.Quantité = lignePostée.Quantité;
            ligne.AFixer = lignePostée.AFixer;
            _context.Ligne.Update(ligne);
            return await SaveChangesAsync();
        }

        public async Task<RetourDeService<LigneCLF>> FixeLigne(LigneCLF ligne, decimal àFixer)
        {
            ligne.AFixer = àFixer;
            _context.Ligne.Update(ligne);
            return await SaveChangesAsync(ligne);
        }

        public async Task<RetourDeService> SupprimeLigne(LigneCLF ligne)
        {
            _context.Ligne.Remove(ligne);
            return await SaveChangesAsync();
        }

        /// <summary>
        /// Copie si possible la valeur de Quantité dans AFixer pour chaque ligne
        /// La copie est impossible si Quantité n'est pas défini ou si la ligne est dans une commande
        /// et demande un nombre de pièces d'un produit mesuré en Kg.
        /// </summary>
        /// <param name="iqLignes"></param>
        /// <param name="type"></param>
        /// <returns>null s'il n y pas de lignes où la copie est possible</returns>
        private async Task<RetourDeService> CopieQuantité(IQueryable<LigneCLF> iqLignes, TypeCLF type)
        {
            if (type == TypeCLF.Commande)
            {
                iqLignes = iqLignes
                    .Include(l => l.Produit)
                    // pas copiable si le produit est mesuré en Kg et TypeCommande de la ligne est Unité
                    .Where(l => l.Produit.SCALP != true);
            }
            List<LigneCLF> copiables = await iqLignes.ToListAsync();
            if (copiables.Count == 0)
            {
                return null;
            }
            copiables.ForEach(l => l.AFixer = l.Quantité);
            _context.Ligne.UpdateRange(copiables);
            return await SaveChangesAsync();
        }

        /// <summary>
        /// Copie si possible la valeur de Quantité dans AFixer pour la ligne définie par la key et le type.
        /// La copie est impossible si Quantité n'est pas défini ou si la ligne est dans une commande
        /// et demande un nombre de pièces d'un produit mesuré en Kg.
        /// </summary>
        /// <param name="keyLigne"></param>
        /// <param name="type"></param>
        /// <returns>null si la copie est impossible</returns>
        public async Task<RetourDeService> CopieQuantité(IKeyLigneSansType keyLigne, TypeCLF type)
        {
            IQueryable<LigneCLF> iqLignes = _context.Ligne
               .Where(l => l.Type == type
                    && l.Id == keyLigne.Id && l.No == keyLigne.No
                    && l.ProduitId == keyLigne.ProduitId
                    && l.Date == keyLigne.Date);
            return await CopieQuantité(iqLignes, type);
        }

        /// <summary>
        /// Copie si possible la valeur de Quantité dans AFixer pour chaque ligne du document défini par la key et le type.
        /// La copie est impossible si Quantité n'est pas défini ou si la ligne est dans une commande
        /// et demande un nombre de pièces d'un produit mesuré en Kg.
        /// </summary>
        /// <param name="keyDoc"></param>
        /// <param name="type"></param>
        /// <returns>null s'il n y pas de lignes où la copie est possible</returns>
        public async Task<RetourDeService> CopieQuantité(IKeyDocSansType keyDoc, TypeCLF type)
        {
            IQueryable<LigneCLF> iqLignes = _context.Ligne
               .Where(l => l.Type == type
                    && l.Id == keyDoc.Id && l.No == keyDoc.No);
            return await CopieQuantité(iqLignes, type);
        }

        /// <summary>
        /// Copie si possible la valeur de Quantité dans AFixer pour chaque ligne des documents de la liste.
        /// La copie est impossible si Quantité n'est pas défini ou si la ligne est dans une commande
        /// et demande un nombre de pièces d'un produitmesuré en Kg.
        /// </summary>
        /// <param name="docs"></param>
        /// <param name="type"></param>
        /// <returns>null s'il n y pas de lignes où la copie est possible</returns>
        public async Task<RetourDeService> CopieQuantité(List<DocCLF> docs, TypeCLF type)
        {
            List<LigneCLF> lignes = new List<LigneCLF>();
            docs.ForEach(d => lignes.AddRange(d.Lignes));
            if (type == TypeCLF.Livraison)
            {
                lignes = lignes
                    .Where(l => l.Produit.SCALP != true)
                    .ToList();
            }
            lignes.ForEach(l => l.AFixer = l.Quantité);
            _context.Ligne.UpdateRange(lignes);
            return await SaveChangesAsync();
        }

        /// <summary>
        /// Annule la valeur de AFixer pour chaque ligne du type
        /// </summary>
        /// <param name="iqLignes">IQueryable des lignes à annuler</param>
        /// <param name="type"></param>
        /// <returns>null s'il n y pas de lignes où la copie est possible</returns>
        private async Task<RetourDeService> Annule(IQueryable<LigneCLF> iqLignes, TypeCLF type)
        {
            List<LigneCLF> lignes = await iqLignes
                .Where(l => l.Type == type)
                .ToListAsync();
            lignes.ForEach(l => l.AFixer = 0);
            _context.Ligne.UpdateRange(lignes);
            return await SaveChangesAsync();
        }

        /// <summary>
        /// Annule la valeur de AFixer pour la ligne définie par la key et le type si AFixer n'est pas défini.
        /// </summary>
        /// <param name="keyLigne"></param>
        /// <param name="type"></param>
        /// <returns>null si la copie est impossible</returns>
        public async Task<RetourDeService> Annule(IKeyLigneSansType keyLigne, TypeCLF type)
        {
            IQueryable<LigneCLF> iqLignes = _context.Ligne
                .Where(l => l.Id == keyLigne.Id && l.No == keyLigne.No
                && l.ProduitId == keyLigne.ProduitId
                && l.Date == keyLigne.Date);
            return await Annule(iqLignes, type);
        }

        /// <summary>
        /// Annule la valeur de AFixer pour chaque ligne du document défini par la key et le type
        /// </summary>
        /// <param name="keyDoc"></param>
        /// <param name="type"></param>
        /// <returns>null s'il n y pas de lignes où la copie est possible</returns>
        public async Task<RetourDeService> Annule(IKeyDocSansType keyDoc, TypeCLF type)
        {
            IQueryable<LigneCLF> iqLignes = _context.Ligne
                .Where(l => l.Id == keyDoc.Id && l.No == keyDoc.No);
            return await Annule(iqLignes, type);
        }

        /// <summary>
        /// Annule la valeur de AFixer pour chaque ligne des documents de la liste.
        /// </summary>
        /// <param name="docs"></param>
        /// <param name="type"></param>
        /// <returns>null s'il n y pas de lignes où la copie est possible</returns>
        public async Task<RetourDeService> Annule(List<DocCLF> docs, TypeCLF type)
        {
            List<LigneCLF> lignes = new List<LigneCLF>();
            docs.ForEach(d => lignes.AddRange(d.Lignes));
            lignes.ForEach(l => l.AFixer = 0);
            _context.Ligne.UpdateRange(lignes);
            return await SaveChangesAsync();
        }

        /// <summary>
        /// Crée un document de synthèse à partir des documents de la liste. Fixe le NoGroupe des documents de la liste.
        /// Si le bon virtuel figure dans la liste, supprime le bon virtuel.
        /// L'objet retourné contient un DocCLF contenant uniquement le No et la Date de la synthèse créée.
        /// </summary>
        /// <param name="site"></param>
        /// <param name="idClient"></param>
        /// <param name="docCLFs">les documents à synthétiser incluent leur lignes et ne sont pas traçés par EF Core</param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<RetourDeService<DocCLF>> Synthèse(Site site, uint idClient, List<DocCLF> docCLFs, TypeCLF type)
        {
            // Les No des synthèses d'un type se suivent par site.
            // Dernière synthèse de ce type ou null s'il n'y a jamais eu de synthèse de ce type.
            DocCLF dernier = await _context.Doc
                .Where(d => d.Id == idClient && d.No > 0 && d.Type == type)
                .OrderBy(d => d.No)
                .AsNoTracking()
                .LastOrDefaultAsync();
            DocCLF synthèse = new DocCLF
            {
                Id = idClient,
                No = dernier == null ? 1 : dernier.No + 1,
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
                    ligne.Date = ligne.Produit.Date;
                }
            }

            var x = docCLFs
                // Agrège les lignes de chaque document en un seul IEnumerable
                .Aggregate(new List<LigneCLF>(), (current, next) => current.Concat(next.Lignes).ToList())
                // Regroupe les lignes par ProduitId et Date
                .GroupBy(l => new { l.ProduitId, l.Date });

            IEnumerable<uint> produitIds = x
                .GroupBy(g => g.Key.ProduitId)
                .Select(g1 => g1.Key);
            List<Produit> produits = await _context.Produit
                .AsNoTracking()
                .Where(p => produitIds.Contains(p.Id))
                .Include(p => p.Archives)
                .ToListAsync();

            // Retourne le Prix d'un Produit à une date passée
            decimal prix(Produit produit, DateTime date)
            {
                if (produit.Date == date)
                {
                    return produit.Prix;
                }
                decimal prix = 0;
                ArchiveProduit[] archivesAvantDate = produit.Archives.Where(a => a.Date <= date && a.Prix.HasValue).OrderBy(a => a.Date).ToArray();
                foreach (ArchiveProduit archive in archivesAvantDate)
                {
                    prix = archive.Prix.Value;
                }
                return prix;
            }
            // Retourne la somme des AFixer des lignes
            decimal quantité(IEnumerable<LigneCLF> lignes)
            {
                decimal quantité = 0;
                foreach (LigneCLF ligne in lignes)
                {
                    quantité += ligne.AFixer.Value;
                }
                return quantité;
            }

            var y = x.Join(produits, g => g.Key.ProduitId, p => p.Id, (g, p) => new
            {
                produit = p,
                date = g.Key.Date,
                quantité = quantité(g.AsEnumerable())
            });
            List<LigneCLF> lignes = new List<LigneCLF>();
            synthèse.Total = 0;
            foreach (var item in y)
            {
                lignes.Add(new LigneCLF
                {
                    Id = synthèse.Id,
                    No = synthèse.No,
                    ProduitId = item.produit.Id,
                    Type = type,
                    Quantité = item.quantité,
                    Date = item.date
                });
                decimal prixApplicable = prix(item.produit, item.date);
                synthèse.Total += prixApplicable * item.quantité;
            }
            synthèse.NbLignes = lignes.Count();

            _context.Doc.Add(synthèse);
            RetourDeService<DocCLF> retour = await SaveChangesAsync(synthèse);

            if (retour.Ok)
            {
                _context.Ligne.AddRange(lignes);
                DocCLF bonVirtuel = docCLFs.Where(d => d.No == 0).FirstOrDefault();
                if (bonVirtuel != null)
                {
                    _context.Doc.Remove(bonVirtuel);
                    docCLFs.Remove(bonVirtuel);
                }
                foreach (DocCLF docCLF in docCLFs)
                {
                    docCLF.NoGroupe = synthèse.No;
                }
                _context.Doc.UpdateRange(docCLFs);
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

        #region Pdf

        public async Task<CLFPdfAEnvoyer> CLFPdfAEnvoyer(KeyDoc keyDocument, bool utilisateurEstLeClient)
        {
            DocCLF docCLF = await _context.Doc
                 .Where(d => d.Id == keyDocument.Id && d.No == keyDocument.No && d.Type == keyDocument.Type)
                 .Include(d => d.Client).ThenInclude(c => c.Site).ThenInclude(s => s.Fournisseur)
                 .Include(d => d.Lignes).ThenInclude(l => l.Produit).ThenInclude(p => p.Archives)
                 .Include(d => d.Téléchargements)
                 .AsNoTracking()
                 .FirstOrDefaultAsync();
            if (docCLF == null)
            {
                return null;
            }

            Client client = new Client { Id = docCLF.Id };
            // Reconstitue le Client à la Date du document
            List<ArchiveClient> archivesClient = await _context.ArchiveClient
                .Where(a => a.Id == client.Id)
                .OrderBy(a => a.Date)
                .ToListAsync();
            if (archivesClient.Last().Date > docCLF.Date.Value)
            {
                archivesClient.Where(a => a.Date <= docCLF.Date.Value).ToList().ForEach(archive =>
                {
                    Client.CopieDataSiPasNull(archive, client);
                });
            }
            else
            {
                Client.CopieData(docCLF.Client, client);
            }
            Role.CopiePréférences(docCLF.Client, client);

            client.SiteId = docCLF.Client.SiteId;
            client.Site = new Site { Id = client.SiteId };
            // Reconstitue le Site à la Date du document
            List<ArchiveSite> archivesSite = await _context.ArchiveSite
                .Where(a => a.Id == client.SiteId)
                .OrderBy(a => a.Date)
                .ToListAsync();
            if (archivesSite.Last().Date > docCLF.Date.Value)
            {
                archivesSite.Where(a => a.Date <= docCLF.Date.Value).ToList().ForEach(archive =>
                {
                    Site.CopieDataSiPasNull(archive, client.Site);
                });
            }
            else
            {
                Site.CopieData(docCLF.Client.Site, client.Site);
            }

            client.Site.Fournisseur = new Fournisseur { Id = client.SiteId };
            // Reconstitue le Fournisseur à la Date du document
            List<ArchiveFournisseur> archivesFournisseur = await _context.ArchiveFournisseur
                .Where(a => a.Id == client.SiteId)
                .OrderBy(a => a.Date)
                .ToListAsync();
            if (archivesFournisseur.Last().Date > docCLF.Date.Value)
            {
                archivesFournisseur.Where(a => a.Date <= docCLF.Date.Value).ToList().ForEach(archive =>
                {
                    Fournisseur.CopieDataSiPasNull(archive, client.Site.Fournisseur);
                });
            }
            else
            {
                Fournisseur.CopieData(docCLF.Client.Site.Fournisseur, client.Site.Fournisseur);
            }
            Role.CopiePréférences(docCLF.Client.Site.Fournisseur, client.Site.Fournisseur);

            // Change si besoin le Produit inclus dans la ligne par sa valeur à la date de la ligne
            foreach (LigneCLF ligne in docCLF.Lignes)
            {
                Produit produit = new Produit { Id = ligne.ProduitId };
                if (ligne.Date < ligne.Produit.Date)
                {
                    // le Produit a été modifié depuis l'enregistrement du document qui a fixé la date de la ligne à celle de son Produit
                    List<ArchiveProduit> archivesAvantDateLigne = ligne.Produit.Archives
                        .Where(a => a.Date <= ligne.Date)
                        .OrderBy(a => a.Date)
                        .ToList();
                    ArchiveProduit archive = null;
                    for (int i = 0; i < archivesAvantDateLigne.Count; i++)
                    {
                        archive = archivesAvantDateLigne[i];
                        Produit.CopieDataSiPasNull(archive, produit);
                    }
                    produit.Date = archive.Date;
                }
                else
                {
                    // produit n'a pas de Date
                    Produit.CopieData(ligne.Produit, produit);
                }
                ligne.Produit = produit;
            }

            // Regroupe les lignes par l'Id de la Catégorie du Produit et par Date
            var x = docCLF.Lignes
                .GroupBy(ligne => new { Id = ligne.Produit.CategorieId, ligne.Date })
                .ToList();

            // Pour chacun des groupes, fixe la Catégorie des produits des lignes en retrouvant si besoin l'état de la Catégorie à la Date du groupe
            foreach (var item in x)
            {
                Catégorie catégorieAvecArchives = await _context.Catégorie
                    .Where(c => c.Id == item.Key.Id)
                    .Include(c => c.Archives)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
                Catégorie catégorie = new Catégorie { Id = item.Key.Id };
                if (item.Key.Date < catégorieAvecArchives.Date)
                {
                    // la Catégorie a changé depuis l'enregistrement
                    List<ArchiveCatégorie> archives = catégorieAvecArchives.Archives
                        .Where(a => a.Date <= item.Key.Date)
                        .OrderBy(a => a.Date)
                        .ToList();
                    foreach (ArchiveCatégorie archive in archives)
                    {
                        Catégorie.CopieDataSiPasNull(archive, catégorie);
                    }
                }
                else
                {
                    Catégorie.CopieData(catégorieAvecArchives, catégorie);
                }

                foreach (LigneCLF ligne in item)
                {
                    ligne.Produit.Catégorie = catégorie;
                }
            }
            CLFPdfDoc pdfDoc = new CLFPdfDoc
            {
                Client = client,
                Type = docCLF.Type,
                Date = docCLF.Date.Value,
                No = docCLF.No,
                Téléchargé = docCLF.Téléchargements.Where(t => t.ParLeClient == utilisateurEstLeClient).Select(t => t.Date).ToList(),
                Lignes = docCLF.Lignes.Select(ligne => new CLFPdfLigne(ligne)).ToList(),
                Téléchargements = utilisateurEstLeClient
                    ? await _context.Téléchargement
                        .Where(t => t.Id == client.Id)
                        .CountAsync()
                    : await _context.Téléchargement
                        .Include(t => t.DocCLF).ThenInclude(d => d.Client)
                        .Where(t => t.DocCLF.Client.SiteId == client.SiteId)
                        .CountAsync()
            };

            return pdfDoc.CLFPdfAEnvoyer(utilisateurEstLeClient);
        }

        public async Task<RetourDeService> Téléchargement(KeyDoc keyDocument, bool utilisateurEstLeClient)
        {
            bool docExiste = await _context.Doc
                .Where(d => d.Id == keyDocument.Id && d.No == keyDocument.No && d.Type == keyDocument.Type)
                .AnyAsync();
            if (!docExiste)
            {
                return null;
            }
            Téléchargement téléchargement = new Téléchargement
            {
                Id = keyDocument.Id,
                No = keyDocument.No,
                Type = keyDocument.Type,
                Date = keyDocument.Téléchargé.Value,
                ParLeClient = utilisateurEstLeClient
            };
            _context.Téléchargement.Add(téléchargement);
            return await SaveChangesAsync();
        }

        #endregion

    }
}
