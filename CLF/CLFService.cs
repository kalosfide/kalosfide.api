using KalosfideAPI.Catalogues;
using KalosfideAPI.Catégories;
using KalosfideAPI.Clients;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages;
using KalosfideAPI.Produits;
using KalosfideAPI.Utiles;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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

        /// <summary>
        /// Retourne un IQueryable qui renvoie les commandes passant les filtres présents et qui inclut les champs Détails, Livraison et Livraison.Facture
        /// et éventuellement (voir les paramètres) les champs Client et Client.Role
        /// </summary>
        /// <param name="filtreDoc">si présent, la Commande doit passer le filtre</param>
        /// <param name="filtreLigne">si présent, au moins un détail doit passer le filtre</param>
        /// <param name="filtreRole">si présent, les champs Client et Client.Role sont inclus et le role doit passer le filtre</param>
        /// <param name="keySite">si présent, les champs Client et Client.Role sont inclus et le site du role doit être celui de keySite</param>
        /// <returns></returns>
        public IQueryable<DocCLF> DocsAvecLignesEtSynthèses(
            Func<DocCLF, bool> filtreDoc, Func<LigneCLF, bool> filtreLigne,
            Func<Role, bool> filtreRole, AKeyUidRno keySite)
        {
            IQueryable<DocCLF> query = _context.Docs;
            if (filtreDoc != null)
            {
                query = query.Where(c => filtreDoc(c));
            }
            if (keySite != null || filtreRole != null)
            {
                query = query.Include(c => c.Client).ThenInclude(cl => cl.Role);
                if (filtreRole != null)
                {
                    query = query.Where(c => filtreRole(c.Client.Role));
                }
                if (keySite != null)
                {
                    query = query.Where(c => _utile.FiltreSite(keySite)(c.Client.Role));
                }
            }
            query = query.Include(c => c.Lignes);
            if (filtreLigne != null)
            {
                query = query.Where(c => c.Lignes.Where(l => filtreLigne(l)).Any());
            }
            return query;
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
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Retourne la ligne définie par la clé et le type avec son document.
        /// </summary>
        /// <param name="ligne"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<LigneCLF> LigneCLFDeKey(AKeyUidRnoNo2 ligne, string type)
        {
            return await _context.Lignes
                .Where(d => d.Uid == ligne.Uid && d.Rno == ligne.Rno && d.No == ligne.No 
                && d.Uid2 == ligne.Uid2 && d.Rno2 == ligne.Rno2 && d.No2 == ligne.No2
                && d.Type == type)
                .Include(l => l.Doc)
                .FirstOrDefaultAsync();
        }

        public async Task<DocCLF> DernierDoc(AKeyUidRno keyClient, string type)
        {
            return await _context.Docs
                .Where(d => d.Uid == keyClient.Uid && d.Rno == keyClient.Rno && d.No > 0 && d.Type == type)
                .OrderBy(d =>  d.No)
                .Include(d => d.Lignes)
                .ThenInclude(l => l.Produit)
                .LastOrDefaultAsync();
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
            string typeBon = type == "L" ? "C" : type == "F" ? "L" : null;
            var docs = await _context.Docs
                .Where(d => d.SiteUid == site.Uid && d.SiteRno == site.Rno && d.Type == typeBon)
                .Where(d => d.Date.HasValue && !d.NoGroupe.HasValue)
                .Include(d => d.Lignes)
                .Select(d => new CLFDoc
                {
                    Uid = d.Uid,
                    Rno = d.Rno,
                    No = d.No,
                    Date = d.Date,
                    NbLignes = d.Lignes.Count,
                    Incomplet = !d.Lignes.All(l => l.AFixer.HasValue)
                }).ToListAsync();
            return new CLFDocs
            {
                Documents = docs
            };
        }

        /// <summary>
        /// Retourne un CLFDocs dont le champ Site contient uniquement Etat et le champ Catalogue uniquement Date
        /// </summary>
        /// <param name="site">le Site du contexte</param>
        /// <returns></returns>
        public async Task<CLFDocs> Contexte(Site site)
        {
            CLFDocs docs = new CLFDocs
            {
                Site = new Site { Etat = site.Etat },
                Catalogue = new Catalogue
                {
                    Date = await _catalogue.DateCatalogue(site, null)
                }
            };
            return docs;
        }

        /// <summary>
        /// Retourne un CLFDocs dont le champ Documents contient les données pour client de la dernière commande d'un client
        /// </summary>
        /// <param name="keyClient">key du client</param>
        /// <returns></returns>
        public async Task<CLFDocs> CommandeEnCours(AKeyUidRno keyClient)
        {
            List<CLFDoc> docs = await _context.Docs
                .Where(d => d.Uid == keyClient.Uid && d.Rno == keyClient.Rno && d.Type == "C")
                .OrderByDescending(d => d.No)
                .Take(1)
                .Include(d => d.Lignes)
                .Select(d => new CLFDoc
                {
                    Uid = d.Uid,
                    Rno = d.Rno,
                    No = d.No,
                    Date = d.Date,
                    NoGroupe = d.NoGroupe,
                    Lignes = d.Lignes.Select(l => new CLFLigneData
                    {
                        No = l.No2,
                        TypeCommande = l.TypeCommande,
                        Quantité = l.Quantité,
                    }).ToList(),
                })
                .ToListAsync();
            CLFDocs clfDocs = new CLFDocs
            {
                Documents = docs

            };
            return clfDocs;
        }

        /// <summary>
        /// Retourne un CLFDocs dont le champ Documents contient les documents envoyés et sans synthèse du client avec les lignes
        /// ou, s'il ny en a pas, la dernière synthèse avec le No égal à 0.
        /// </summary>
        /// <param name="site"></param>
        /// <param name="keyClient"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<CLFDocs> BonsDUnClient(Site site, AKeyUidRno keyClient, string type)
        {
            string typeBon = type == "L" ? "C" : type == "F" ? "L" : null;
            DocCLF[] bons = await _context.Docs
                .Where(d => d.Uid == keyClient.Uid && d.Rno == keyClient.Rno && d.Type == typeBon)
                .Where(d => d.Date.HasValue && !d.NoGroupe.HasValue)
                .Include(d => d.Lignes).ThenInclude(l => l.ArchiveProduit)
                .ToArrayAsync();

            // Retourne le LigneCLFTarif d'une LigneCLF contenant la ligne et son ArchiveProduit
            async Task<LigneCLFTarif> ligneDernièreArchiveProduit(LigneCLF ligne)
            {
                var ap1 = await _context.Produit
                    .Where(p => p.Uid == ligne.Uid2 && p.Rno == ligne.Rno2 && p.No == ligne.No2)
                    .Include(p => p.ArchiveProduits)
                    .Select(p => p.ArchiveProduits)
                    .FirstAsync();
                var ap = ap1.ToList().Last();
                return new LigneCLFTarif
                {
                    Ligne = ligne,
                    ArchiveProduit = ap
                };
            }

            // Retourne le DocCLFLignesCLFTarifs d'un document contenant le document
            // et les LigneCLFTarif contenant les LigneCLF du document et leurs ArchiveProduit
            async Task<DocCLFLignesCLFTarifs> docCLFLignesDernièresArchivesProduit(DocCLF doc)
            {
                LigneCLFTarif[] lignesCLFTarifs = await Task.WhenAll(doc.Lignes.Select(l => ligneDernièreArchiveProduit(l)).ToArray());
                return new DocCLFLignesCLFTarifs
                {
                    DocCLF = doc,
                    LigneCLFTarifs = lignesCLFTarifs
                };
            }

            async Task<DocCLFLignesCLFTarifs> docCLFLignesTarifs(DocCLF doc)
            {
                DocCLFLignesCLFTarifs docCLFLignesCLFTarifs = await docCLFLignesDernièresArchivesProduit(doc);
                docCLFLignesCLFTarifs.LigneCLFTarifs = docCLFLignesCLFTarifs.LigneCLFTarifs.Where(lt => lt.Ligne.Date < lt.ArchiveProduit.Date);
                if (docCLFLignesCLFTarifs.LigneCLFTarifs.Count() == 0)
                {
                }
                return docCLFLignesCLFTarifs;
            }

            var x2 = await Task.WhenAll(bons.Select(d => docCLFLignesTarifs(d)).ToArray());

            async Task<LigneCLFTarif> ligneClFArchiveCatégorie(LigneCLF ligne)
            {
                var archives = await _context.Catégorie
                    .Where(c => c.Uid == ligne.Uid2 && c.Rno == ligne.Rno2 && c.No == ligne.ArchiveProduit.CategorieNo)
                    .Include(c => c.ArchiveCatégories)
                    .Select(c => c.ArchiveCatégories
                        .Where(ac => ac.Date <= ligne.Date))
                    .FirstAsync();
                ArchiveCatégorie archive = archives.OrderBy(a => a.Date).Last();
                return new LigneCLFTarif
                {
                    Ligne = ligne,
                    ArchiveCatégorie = archive
                };
            }

            async Task<DocCLFTarif> docCLFTarifs1(DocCLF doc)
            {
                DocCLFLignesCLFTarifs docCLFLignesCLFTarifs = await docCLFLignesDernièresArchivesProduit(doc);
                var ligneCLFTarifs = docCLFLignesCLFTarifs.LigneCLFTarifs.Where(lt => lt.Ligne.Date < lt.ArchiveProduit.Date);
                if (ligneCLFTarifs.Count() > 0)
                {
                    ligneCLFTarifs = await Task.WhenAll(docCLFLignesCLFTarifs.LigneCLFTarifs
                        .Select(lt => ligneClFArchiveCatégorie(lt.Ligne))
                        .ToArray());
                }
                return new DocCLFTarif
                {
                    DocCLF = doc,
                    ArchivesProduit = ligneCLFTarifs.Select(lt => lt.Ligne.ArchiveProduit),
                    ArchivesCatégorie = ligneCLFTarifs.Select(lt => lt.ArchiveCatégorie)
                };
            }

            DocCLFTarif[] docCLFTarifs = await Task.WhenAll(bons.Select(d => docCLFTarifs1(d)).ToArray());

            List<CLFDoc> docs = docCLFTarifs
                .Select(daspc => new CLFDoc
                {
                    Uid = daspc.DocCLF.Uid,
                    Rno = daspc.DocCLF.Rno,
                    No = daspc.DocCLF.No,
                    Date = daspc.DocCLF.Date,
                    Lignes = daspc.DocCLF.Lignes.Select(l => new CLFLigneData
                    {
                        No = l.No2,
                        Date = l.Date,
                        TypeCommande = l.TypeCommande,
                        Quantité = l.Quantité,
                        AFixer = l.AFixer
                    }).ToList(),
                    Tarif = new Catalogue
                    {
                        Produits = daspc.ArchivesProduit.Select(p => _produitService.CréeProduitDataAvecDate(p)).ToList(),
                        Catégories=daspc.ArchivesCatégorie.Select(c => _catégorieService.CréeCatégorieDataAvecDate(c)).ToList()
                    }
                })
                .ToList();

            if (docs.Count() == 0)
            {
                DocCLF docCLF = await _context.Docs
                    .Where(d => d.Uid == keyClient.Uid && d.Rno == keyClient.Rno && d.Type == type)
                    .Where(d => d.Date.HasValue)
                    .OrderBy(d => d.Date)
                    .Include(d => d.Lignes).ThenInclude(l => l.ArchiveProduit).ThenInclude(a => a.Produit).ThenInclude(p => p.ArchiveProduits)
                    .LastOrDefaultAsync();
                if (docCLF != null)
                {
                    var lignesArchives = docCLF.Lignes
                            .Select(l => new {
                                ligne = l,
                                dernièreArchiveProduit = l.ArchiveProduit.Produit.ArchiveProduits.OrderBy(a => a.Date).Last(),
                            })
                            .ToArray();
                    IEnumerable<LigneCLF> lignes = docCLF.Lignes
                            .Select(l => new {
                                ligne = l,
                                dernièreArchiveProduit = l.ArchiveProduit.Produit.ArchiveProduits.OrderBy(a => a.Date).Last(),
                            })
                            .Where(la => la.dernièreArchiveProduit.Etat == TypeEtatProduit.Disponible)
                            .Select(la => la.ligne);

                    docs = new List<CLFDoc>
                    {
                        new CLFDoc
                        {
                            Uid = docCLF.Uid,
                            Rno = docCLF.Rno,
                            No = 0,
                            Type = docCLF.Type,
                            Date = docCLF.Date,
                            Lignes = lignes.Select(l => new CLFLigneData
                            {
                                No = l.No2,
                                Quantité = l.Quantité,
                            }).ToList(),
                        }
                        };
                }
            }
            return new CLFDocs
            {
                Documents = docs
            };
        }

        /// <summary>
        /// Retourne la liste des documents (avec leurs lignes) du type demandé dont le client est celui du clfDocs et le numéro l'un de ceux des documents du clfDocs.
        /// Les DocCLF retounés incluent leurs LigneCLF et, pour les commandes, les lignes incluent leurs Produit.
        /// </summary>
        /// <param name="clfDocs"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<List<DocCLF>> DocumentsEnvoyésSansSynthèse(CLFDocsSynthèse clfDocs, string type)
        {
            IQueryable<DocCLF> queryDocs = _context.Docs
                .Where(d => d.Uid == clfDocs.KeyClient.Uid && d.Rno == clfDocs.KeyClient.Rno && d.Type == type)
                .Where(d => d.Date.HasValue && !d.NoGroupe.HasValue)
                .Where(d => clfDocs.NoDocs.Where(no => no == d.No).Any());
            if (type == "C")
            {
                queryDocs = queryDocs.Include(d => d.Lignes).ThenInclude(l => l.Produit);
            }
            else
            {
                queryDocs = queryDocs.Include(d => d.Lignes);
            }
            return await queryDocs.ToListAsync();
        }

        /// <summary>
        /// Retourne un CLFDocs contenant la liste des résumés des documents envoyés du client ou du site suivant lequel des paramètres est non null
        /// qui vérifient les conditions de type et de date définies par paramsFiltre.
        /// La liste est dans l'ordre inverse des dates et contient paramsFiltre.Nb documents si paramsFiltre.Nb est défini.
        /// </summary>
        /// <param name="paramsFiltre">définit le nombre de documents à retourner et les conditions de type et de Date</param>
        /// <param name="site"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        private async Task<CLFDocs> Résumés(ParamsFiltreDoc paramsFiltre, Site site, Client client)
        {
            IQueryable<DocCLF> query = _context.Docs.Where(d => d.Date.HasValue && !DateNulle.Egale(d.Date.Value));
            if (site != null)
            {
                query = query.Where(d => d.SiteUid == site.Uid && d.SiteRno == site.Rno);
            }
            if (client != null)
            {
                query = query.Where(d => d.Uid == client.Uid && d.Rno == client.Rno);
            }
            if (paramsFiltre.Type != null)
            {
                query = query.Where(d => d.Type == paramsFiltre.Type);
            }
            if (paramsFiltre.DateMin != null)
            {
                query = query.Where(d => DateTime.Compare(d.Date.Value, paramsFiltre.DateMin.Value) >= 0);
            }
            if (paramsFiltre.DateMax != null)
            {
                query = query.Where(d => DateTime.Compare(d.Date.Value, paramsFiltre.DateMax.Value)<= 0);
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
            List<CLFDoc> docs = await query.Select(d => new CLFDoc
                {
                    Uid = d.Uid,
                    Rno = d.Rno,
                    No = d.No,
                    Type = d.Type,
                    Date = d.Date,
                    NoGroupe = d.NoGroupe,
                    NbLignes = d.NbLignes,
                    Total = d.Total,
                    Incomplet = d.Incomplet
                })
                .ToListAsync();
            return new CLFDocs
            {
                Documents = docs
            };
        }

        /// <summary>
        /// Retourne un CLFDocs contenant la liste des résumés des documents envoyés du site
        /// qui vérifient les conditions de type et de date définies par paramsFiltre.
        /// La liste est dans l'ordre inverse des dates et contient paramsFiltre.Nb documents si paramsFiltre.Nb est défini.
        /// </summary>
        /// <param name="paramsFiltre">définit le nombre de documents à retourner et les conditions de type et de Date</param>
        /// <param name="site"></param>
        /// <returns></returns>
        public async Task<CLFDocs> Résumés(ParamsFiltreDoc paramsFiltre, Site site)
        {
            return await Résumés(paramsFiltre, site, null);
        }

        /// <summary>
        /// Retourne un CLFDocs contenant la liste des résumés des documents envoyés du client
        /// qui vérifient les conditions de type et de date définies par paramsFiltre.
        /// La liste est dans l'ordre inverse des dates et contient paramsFiltre.Nb documents si paramsFiltre.Nb est défini.
        /// </summary>
        /// <param name="paramsFiltre">définit le nombre de documents à retourner et les conditions de type et de Date</param>
        /// <param name="client"></param>
        /// <returns></returns>
        public async Task<CLFDocs> Résumés(ParamsFiltreDoc paramsFiltre, Client client)
        {
            return await Résumés(paramsFiltre, null, client);
        }

        public async Task<DocCLF> Document(AKeyUidRnoNo keyDocument, string type)
        {
            var docs = _context.Docs
                .Where(d => d.Uid == keyDocument.Uid && d.Rno == keyDocument.Rno && d.No == keyDocument.No && d.Type == type)
                .Where(d => d.Date.HasValue);
            return await docs.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Retourne un CLFDocs qui contient le Client du document et un Documents contenant le document avec ses lignes et son tarif
        /// </summary>
        /// <param name="site"></param>
        /// <param name="keyDocument"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<CLFDocs> Document(Site site, KeyUidRnoNo keyDocument, string type)
        {
            DocCLF docCLF = await _context.Docs
                .Where(d => d.Uid == keyDocument.Uid && d.Rno == keyDocument.Rno && d.No == keyDocument.No && d.Type == type)
                .Where(d => d.Date.HasValue)
                .Include(d => d.Lignes).ThenInclude(l => l.ArchiveProduit)
                .Include(d => d.Client)
                .FirstOrDefaultAsync();
            List<ProduitData> produits = docCLF.Lignes.Select(l => _produitService.CréeProduitDataAvecDate(l.ArchiveProduit)).ToList();
            List<CatégorieData> catégories = await _context.ArchiveCatégorie
                .Where(c => produits.Where(p => p.CategorieNo == c.No).Any())
                .GroupBy(c => new { c.Uid,c.Rno,c.No })
                .Select(gr => gr.OrderBy(c => c.Date).Last())
                .Select(c => _catégorieService.CréeCatégorieDataAvecDate(c))
                .ToListAsync();
            CLFDoc clfDoc = new CLFDoc
            {
                Uid = docCLF.Uid,
                Rno = docCLF.Rno,
                No = docCLF.No,
                Date = docCLF.Date,
                Lignes = docCLF.Lignes.Select(l => new CLFLigneData
                {
                    No = l.No2,
                    Date = l.Date,
                    TypeCommande = l.TypeCommande,
                    Quantité = l.Quantité,
                }).ToList(),
                Tarif = new Catalogue
                {
                    Produits = produits,
                    Catégories = catégories
                },
                Total = docCLF.Total,
            };
            Client client = new Client
            {
                Uid = docCLF.Uid,
                Rno = docCLF.Rno,
                Nom = docCLF.Client.Nom,
                Adresse = docCLF.Client.Adresse
            };
            return new CLFDocs
            {
                Client = client,
                Documents = new List<CLFDoc>
                {
                    clfDoc
                }
            };
        }

        #endregion // Lecture

        #region Action

        public async Task<RetourDeService<CLFDoc>> AjouteBon(AKeyUidRno keyClient, Site site, string type, long noDoc, DocCLF docACopier)
        {
            DocCLF doc = new DocCLF
            {
                Uid = keyClient.Uid,
                Rno = keyClient.Rno,
                No = noDoc,
                SiteUid = site.Uid,
                SiteRno = site.Rno,
                Type = type
            };
            CLFDoc clfDoc = new CLFDoc
            {
                No = noDoc
            };
            if (noDoc == 0)
            {
                doc.Date = DateNulle.Date;
                clfDoc.Date = DateNulle.Date;
            }
            _context.Docs.Add(doc);
            RetourDeService<CLFDoc> retour = await SaveChangesAsync(clfDoc);

            if (retour.Ok && docACopier != null)
            {
                LigneCLF Copie(LigneCLF ligne, long NoDoc)
                {
                    LigneCLF copie = new LigneCLF
                    {
                        Quantité = ligne.Quantité,
                        Type = type,
                        Date = ligne.Date
                    };
                    copie.CopieKey(ligne.KeyParam);
                    copie.No = NoDoc;
                    return copie;
                }
                IEnumerable<LigneCLF> lignes = docACopier.Lignes.ToArray();
                if (type == "C")
                {
                    lignes = lignes.Where(l => l.Produit.Etat == TypeEtatProduit.Disponible);
                }
                lignes = lignes.Select(l => Copie(l, noDoc));
                _context.Lignes.AddRange(lignes);
                RetourDeService retour1 = await SaveChangesAsync();
                if (!retour1.Ok)
                {
                    retour.Type = retour1.Type;
                    retour.Message = retour1.Message;
                }
            }

            return retour;
        }

        public async Task<RetourDeService> SupprimeCommande(DocCLF doc)
        {
            _context.Docs.Remove(doc);
            // vérifier que les lignes sont supprimées
            return await SaveChangesAsync();
        }

        public async Task<RetourDeService<CLFDoc>> EnvoiCommande(DocCLF doc)
        {
            doc.Date = DateTime.Now;
            doc.NbLignes = doc.Lignes.Count;
            doc.Total = 0;
            void Ajoute(LigneCLF ligne)
            {
                if (ligne.TypeCommande == TypeUnitéDeCommande.Unité && ligne.Produit.TypeCommande==TypeUnitéDeCommande.UnitéOuVrac)
                {
                    doc.Incomplet = true;
                }
                else
                {
                    doc.Total += ligne.Quantité * ligne.Produit.Prix;
                }
            }
            doc.Lignes.ToList().ForEach(l => Ajoute(l));
            _context.Docs.Update(doc);
            CLFDoc docRetour = new CLFDoc
            {
                No = doc.No,
                Date = doc.Date
            };
            return await SaveChangesAsync(docRetour);
        }

        public async Task<RetourDeService<CLFLigne>> AjouteLigne(CLFLigne ligne)
        {
            LigneCLF ajout = new LigneCLF
            {
                Quantité = ligne.Quantité,
                AFixer = ligne.AFixer,
                Type = "C",
                TypeCommande = ligne.TypeCommande,
                Date = ligne.Date.Value
            };
            ajout.CopieKey(ligne.KeyParam);
            _context.Lignes.Add(ajout);
            return await SaveChangesAsync(ligne);
        }

        public async Task<RetourDeService<LigneCLF>> EditeLigne(LigneCLF ligne, CLFLigne lignePostée)
        {
            ligne.Quantité = lignePostée.Quantité;
            ligne.AFixer = lignePostée.AFixer;
            ligne.TypeCommande = lignePostée.TypeCommande;
            _context.Lignes.Update(ligne);
            return await SaveChangesAsync(ligne);
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
        /// Copie si possible la valeur de Quantité dans AFixer pour chaque ligne du type qui passe le filtre.
        /// La copie est impossible si Quantité n'est pas défini ou si la ligne est dans une commande
        /// et demande un nombre de pièces d'un produit dont le prix dépend d'une mesure.
        /// </summary>
        /// <param name="filtre"></param>
        /// <param name="type"></param>
        /// <returns>null s'il n y pas de lignes où la copie est possible</returns>
        private async Task<RetourDeService> CopieQuantité(Func<LigneCLF, bool> filtre, string type)
        {
            var lignes = _context.Lignes
                .Where(l => filtre(l) && l.Type == type);
            if (type == "C")
            {
                lignes = lignes
                    .Include(l => l.Produit)
                    .Where(l => l.Produit.TypeCommande != TypeUnitéDeCommande.UnitéOuVrac || l.TypeCommande == TypeUnitéDeCommande.Vrac);
            }
            var copiables = await lignes.ToListAsync();
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
            bool filtre(LigneCLF l) => l.Uid == keyLigne.Uid && l.Rno == keyLigne.Rno && l.No == keyLigne.No
                && l.Uid2 == keyLigne.Uid2 && l.Rno2 == keyLigne.Rno2 && l.No2 == keyLigne.No2;
            return await CopieQuantité(filtre, type);
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
            bool filtre(LigneCLF l) => l.Uid == keyDoc.Uid && l.Rno == keyDoc.Rno && l.No == keyDoc.No;
            return await CopieQuantité(filtre, type);
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
            if (type == "L")
            {
                lignes = lignes
                    .Where(l => l.Produit.TypeCommande != TypeUnitéDeCommande.UnitéOuVrac || l.TypeCommande == TypeUnitéDeCommande.Vrac)
                    .ToList();
            }
            lignes.ForEach(l => l.AFixer = l.Quantité);
            _context.Lignes.UpdateRange(lignes);
            return await SaveChangesAsync();
        }

        /// <summary>
        /// Annule la valeur de AFixer pour chaque ligne du type qui passe le filtre.
        /// </summary>
        /// <param name="filtre"></param>
        /// <param name="type"></param>
        /// <returns>null s'il n y pas de lignes où la copie est possible</returns>
        private async Task<RetourDeService> Annule(Func<LigneCLF, bool> filtre, string type)
        {
            List<LigneCLF> lignes = await _context.Lignes
                .Where(l => filtre(l) && l.Type == type)
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
            bool filtre(LigneCLF l) => l.Uid == keyLigne.Uid && l.Rno == keyLigne.Rno && l.No == keyLigne.No
                && l.Uid2 == keyLigne.Uid2 && l.Rno2 == keyLigne.Rno2 && l.No2 == keyLigne.No2
                && !l.AFixer.HasValue;
            return await Annule(filtre, type);
        }

        /// <summary>
        /// Annule la valeur de AFixer pour chaque ligne du document défini par la key et le type dont le AFixer n'est pas défini.
        /// </summary>
        /// <param name="keyDoc"></param>
        /// <param name="type"></param>
        /// <returns>null s'il n y pas de lignes où la copie est possible</returns>
        public async Task<RetourDeService> Annule(AKeyUidRnoNo keyDoc, string type)
        {
            bool filtre(LigneCLF l) => l.Uid == keyDoc.Uid && l.Rno == keyDoc.Rno && l.No == keyDoc.No && !l.AFixer.HasValue;
            return await Annule(filtre, type);
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
        /// L'objet retourné contient un DocCLF contenant uniquement le No et la Date de la synthèse créée.
        /// </summary>
        /// <param name="docCLFs"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<RetourDeService<DocCLF>> Synthèse(List<DocCLF> docCLFs, string type)
        {
            KeyUidRno keyClient = new KeyUidRno
            {
                Uid = docCLFs[0].Uid,
                Rno = docCLFs[0].Rno
            };
            KeyUidRno keySite = new KeyUidRno
            {
                Uid = docCLFs[0].SiteUid,
                Rno = docCLFs[0].SiteRno
            };
            DocCLF dernier = await DernierDoc(keyClient, type);
            DocCLF synthèse = new DocCLF
            {
                Uid = keyClient.Uid,
                Rno = keyClient.Rno,
                No = dernier == null ? 1 : dernier.No + 1,
                SiteUid = keySite.Uid,
                SiteRno = keySite.Rno,
                Type = type,
                Date = DateTime.Now
            };

            List<LigneCLF> lignesCLF = new List<LigneCLF>();
            void AjouteCLFLigne(LigneCLF ligneCLF) 
                {
                LigneCLF ligneSynthèse = lignesCLF.Where(l => l.No2 == ligneCLF.No2 && l.Date == ligneCLF.Date).FirstOrDefault();
                if (ligneSynthèse != null)
                {
                    ligneSynthèse.Quantité += ligneCLF.AFixer;
                }
                else
                {
                    ligneSynthèse = new LigneCLF
                    {
                        Uid = synthèse.Uid,
                        Rno = synthèse.Rno,
                        No = synthèse.No,
                        Uid2 = ligneCLF.Uid2,
                        Rno2 = ligneCLF.Rno2,
                        No2 = ligneCLF.No2,
                        Type = type,
                        Quantité = ligneCLF.AFixer,
                        Date = ligneCLF.Date
                    };
                    lignesCLF.Add(ligneSynthèse);
                }
            }
            void AjouteCLFDoc(DocCLF docCLF)
            {
                docCLF.NoGroupe = synthèse.No;
                docCLF.Lignes.ToList().ForEach(l => AjouteCLFLigne(l));
            }
            docCLFs.ForEach(d => AjouteCLFDoc(d));

            async Task<decimal> coût(LigneCLF ligneCLF)
            {
                return await _context.ArchiveProduit
                    .Where(ap => ap.Uid == ligneCLF.Uid2 && ap.Rno == ligneCLF.Rno2 && ap.No == ligneCLF.No2 && ap.Date == ligneCLF.Date)
                    .Select(ap => ligneCLF.Quantité.Value * ap.Prix.Value)
                    .FirstAsync();
            }

            synthèse.NbLignes = lignesCLF.Count();
            decimal[] coûts = await Task.WhenAll(lignesCLF.Select(l => coût(l)));
            synthèse.Total = coûts.Sum();

            _context.Docs.Add(synthèse);
            RetourDeService<DocCLF> retour = await SaveChangesAsync(synthèse);

            if (retour.Ok)
            {
                _context.Lignes.AddRange(lignesCLF);
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
