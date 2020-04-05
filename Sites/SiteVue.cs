using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Newtonsoft.Json;

namespace KalosfideAPI.Sites
{
    public class SiteVue : AKeyUidRno
    {
        public override string Uid { get; set; }
        public override int Rno { get; set; }
        public string NomSite { get; set; }
        public string Titre { get; set; }

        /// <summary>
        /// Ville de signature des documents
        /// </summary>
        public string Ville { get; set; }

        /// <summary>
        /// Chaîne de caractère où {no} représente le numéro du document et {client} le nom du client 
        /// </summary>
        public string FormatNomFichierCommande { get; set; }

        /// <summary>
        /// Chaîne de caractère où {no} représente le numéro du document et {client} le nom du client 
        /// </summary>
        public string FormatNomFichierLivraison { get; set; }

        /// <summary>
        /// Chaîne de caractère où {no} représente le numéro du document et {client} le nom du client 
        /// </summary>
        public string FormatNomFichierFacture { get; set; }
        public string Etat { get; set; }

        /// <summary>
        /// nb de produits disponibles
        /// </summary>
        public int? NbProduits { get; set; }

        /// <summary>
        /// nombre de clients d'état Nouveau ou Actif
        /// </summary>
        public int? NbClients { get; set; }

        public void Copie(Site site)
        {
            Uid = site.Uid;
            Rno = site.Rno;
            NomSite = site.NomSite;
            Titre = site.Titre;
            Etat = site.Etat;
            Ville = site.Ville;
            FormatNomFichierCommande = site.FormatNomFichierCommande;
            FormatNomFichierLivraison = site.FormatNomFichierLivraison;
            FormatNomFichierFacture = site.FormatNomFichierFacture;
        }
    }
}