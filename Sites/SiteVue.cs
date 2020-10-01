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
        public string Url { get; set; }
        public string Titre { get; set; }

        /// <summary>
        /// Pour l'en-tête des documents
        /// </summary>
        public string Nom { get; set; }

        /// <summary>
        /// Pour l'en-tête des documents
        /// </summary>
        public string Adresse { get; set; }

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
            Url = site.Url;
            Titre = site.Titre;
            Etat = site.Etat;
            Nom = site.Nom;
            Adresse = site.Adresse;
            Ville = site.Ville;
            FormatNomFichierCommande = site.FormatNomFichierCommande;
            FormatNomFichierLivraison = site.FormatNomFichierLivraison;
            FormatNomFichierFacture = site.FormatNomFichierFacture;
        }
    }
}