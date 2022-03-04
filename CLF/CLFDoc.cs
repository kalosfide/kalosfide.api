using KalosfideAPI.Catalogues;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.CLF
{
    /// <summary>
    /// Commande, Livraison ou Facture
    /// Rassemble les informations sur une entity DocCLF de la base de donnés.
    /// Objet transmis à l'application-client dans la liste ApiDocs d'un CLFDocs.
    /// </summary>
    public class CLFDoc: IKeyDocSansType
    {
        /// <summary>
        /// Id du Client
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        /// Nom du client
        /// Présent uniquement si le document est envoyé sans ses lignes pour servir à définir un lien.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Nom { get; set; }


        /// <summary>
        /// No du document, incrémenté automatiquement par client pour les commandes, par site pour les livraisons et factures
        /// </summary>
        public uint No { get; set; }

        /// <summary>
        /// 'C' ou 'L' ou 'F'.
        /// Présent uniquement si le document fait partie d'une liste de vues.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public TypeCLF? Type { get; set; }

        // données

        /// <summary>
        /// Date d'enregistrement du document.
        /// Présent si le document a été enregistré.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? Date { get; set; }

        /// <summary>
        /// No de la synthèse qui répond au document.
        /// Présent si le document est un bon qui a été traité dans une synthèse qui a été enregistrée.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long? NoGroupe { get; set; }

        /// <summary>
        /// Date de la synthèse qui répond au document.
        /// Présent si le document est un bon qui a été traité dans une synthèse qui a été enregistrée.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? DateGroupe { get; set; }

        /// <summary>
        /// Liste des No des bons dont ce document fait la synthèse.
        /// Présent si le document est une synthèse qui a été enregistrée.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<uint> NoBons { get; set; }

        /// <summary>
        /// Lignes du document.
        /// Présent si le document n'est pas transmis sous forme de résumé.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ICollection<CLFLigneData> Lignes { get; set; }

        /// <summary>
        /// Nombre de lignes.
        /// Présent si le document est transmis sous forme de résumé.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? NbLignes { get; set; }

        /// <summary>
        /// Coût total des lignes.
        /// Présent si le document a été enregistré et est transmis sous forme de résumé.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? Total { get; set; }

        /// <summary>
        /// Présent et vrai si le document est transmis sous forme de résumé dans une liste à synthétiser et s'il contient des lignes dont le AFixer est null.
        /// Présent et vrai si le document est transmis sous forme de résumé dans une liste de vues et s'il contient des lignes dont le coût n'est pas calculable.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? Incomplet { get; set; }

        private CLFDoc() { }

        /// <summary>
        /// Crée un document à envoyer avec seulement la Date.
        /// </summary>
        /// <param name="docCLF">document dans la bdd</param>
        /// <returns></returns>
        public static CLFDoc DeDate(DocCLF docCLF)
        {
            CLFDoc clfDoc = new CLFDoc
            {
                Date = docCLF.Date
            };
            return clfDoc;
        }

        /// <summary>
        /// Crée un document à envoyer avec seulement le No.
        /// </summary>
        /// <param name="noBon">document dans la bdd</param>
        /// <returns></returns>
        public static CLFDoc DeNo(uint noBon)
        {
            CLFDoc clfDoc = new CLFDoc
            {
                No = noBon
            };
            return clfDoc;
        }

        /// <summary>
        /// Crée un document à envoyer avec seulement l'Id et le nom du client, le No et la Date du document.
        /// </summary>
        /// <param name="docCLF">document dans la bdd</param>
        /// <returns></returns>
        public static CLFDoc DeIdNomNoDate(DocCLF docCLF)
        {
            CLFDoc clfDoc = new CLFDoc
            {
                Id = docCLF.Id,
                Nom = docCLF.Client.Nom,
                No = docCLF.No,
                Date = docCLF.Date
            };
            return clfDoc;
        }

        /// <summary>
        /// Crée un document à envoyer avec seulement le No et le Type.
        /// </summary>
        /// <param name="docCLF">document dans la bdd</param>
        /// <returns></returns>
        public static CLFDoc DeNoType(DocCLF docCLF)
        {
            CLFDoc clfDoc = new CLFDoc
            {
                No = docCLF.No,
                Type = docCLF.Type
            };
            return clfDoc;
        }

        private CLFDoc CopieLignes(DocCLF docCLF, Func<LigneCLF, CLFLigneData> créeLigneData)
        {
            Lignes = docCLF.Lignes.Select(l => créeLigneData(l)).ToList();
            return this;
        }
        public CLFDoc CopieLignes(DocCLF docCLF)
        {
            return CopieLignes(docCLF, CLFLigneData.LigneData);
        }
        public CLFDoc CopieLignesAvecAFixer(DocCLF docCLF)
        {
            return CopieLignes(docCLF, CLFLigneData.LigneDataAvecAFixer);
        }

        /// <summary>
        /// Crée un document à envoyer avec la key, la Date, les lignes avec ou sans AFixer suivant la fonction de création de ligne.
        /// </summary>
        /// <param name="docCLF">document dans la bdd</param>
        /// <param name="créeLigneData">transforme une ligne de la bdd en une ligne à envoyer</param>
        /// <returns></returns>
        public static CLFDoc AvecLignes(DocCLF docCLF, Func<LigneCLF, CLFLigneData> créeLigneData)
        {
            CLFDoc clfDoc = new CLFDoc
            {
                Id = docCLF.Id,
                No = docCLF.No,
                Date = docCLF.Date,
                Lignes = docCLF.Lignes.Select(l => créeLigneData(l)).ToList()
            };
            return clfDoc;
        }

        /// <summary>
        /// Crée un document à envoyer avec la key, la Date, les lignes sans AFixer.
        /// </summary>
        /// <param name="docCLF">document dans la bdd</param>
        /// <returns></returns>
        public static CLFDoc AvecLignes(DocCLF docCLF)
        {
            CLFDoc clfDoc = CLFDoc.AvecLignes(docCLF, CLFLigneData.LigneData);
            return clfDoc;
        }

        /// <summary>
        /// Crée un document à envoyer avec la key, la Date, les lignes avec AFixer.
        /// </summary>
        /// <param name="docCLF">document dans la bdd</param>
        /// <returns></returns>
        public static CLFDoc AvecLignesAvecAFixer(DocCLF docCLF)
        {
            CLFDoc clfDoc = CLFDoc.AvecLignes(docCLF, CLFLigneData.LigneDataAvecAFixer);
            return clfDoc;
        }

        /// <summary>
        /// Crée un document à envoyer avec la key, la Date, le Type, le NbLignes, le Total et le Incomplet.
        /// Si le document fait partie d'une synthèse, le CLFDoc contient aussi le NoGroupe, la DateGroupe.
        /// </summary>
        /// <param name="docCLF">document dans la bdd</param>
        /// <returns></returns>
        public static async Task<CLFDoc> Résumé(DocCLF docCLF, DbSet<DocCLF> docs)
        {
            CLFDoc clfDoc = new CLFDoc
            {
                Id = docCLF.Id,
                No = docCLF.No,
                Type = docCLF.Type,
                Date = docCLF.Date,
                NoGroupe = docCLF.NoGroupe,
                NbLignes = docCLF.NbLignes,
                Total = docCLF.Total,
                Incomplet = docCLF.Incomplet
            };
            await clfDoc.FixeSynthèse(docCLF, docs);
            return clfDoc;
        }

        public static CLFDoc APréparer(DocCLF docCLF)
        {
            CLFDoc clfDoc = new CLFDoc
            {
                Id = docCLF.Id,
                No = docCLF.No,
                Date = docCLF.Date,
                NbLignes = docCLF.Lignes.Count,
                Incomplet = !docCLF.Lignes.All(l => l.AFixer.HasValue)
            };
            return clfDoc;
        }

         public async Task FixeSynthèse(DocCLF docCLF, DbSet<DocCLF> docs)
        {
            if (docCLF.NoGroupe != null)
            {
                NoGroupe = docCLF.NoGroupe;
                DateGroupe = await docs
                    .Where(d => d.Id == docCLF.Id && d.No == docCLF.NoGroupe && d.Type == DocCLF.TypeSynthèse(docCLF.Type))
                    .Select(d => d.Date)
                    .FirstOrDefaultAsync();
            }
        }

         public async Task FixeSynthétisés(DocCLF docCLF, DbSet<DocCLF> docs)
        {
            if (docCLF.Type != TypeCLF.Commande)
            {
                NoBons = await docs
                    .Where(d => d.Id == docCLF.Id && d.NoGroupe == docCLF.No && d.Type == DocCLF.TypeBon(docCLF.Type))
                    .Select(d => d.No)
                    .ToListAsync();
            }
        }
    }
}
