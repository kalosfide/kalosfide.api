using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data.Keys
{
    public interface IKeyLigneSansType : IKeyDocSansType
    {

        /// <summary>
        /// Id du Produit.
        /// </summary>
        public uint ProduitId { get; set; }

        /// <summary>
        /// Date du Catalogue applicable à la Ligne.
        /// Quand une ligne est ajoutée à un bon, elle a la date du catalogue au moment de l'ajout.
        /// Quand un bon de commande est envoyé, ses lignes prennent la date du catalogue au moment de l'envoi.
        /// Quand une synthèse est enregistrée, sa date est fixée et les lignes du bon virtuel éventuel sont
        /// incorporées dans la synthèse avec la date de la synthèse et les lignes des autres bons avec la date qu'ils ont déjà.
        /// </summary>
        public DateTime Date { get; set; }
    }

    public class KeyLigneSansType : KeyDocSansType, IKeyLigneSansType
    {

        /// <summary>
        /// Id du Produit.
        /// </summary>
        public uint ProduitId { get; set; }

        /// <summary>
        /// Date du Catalogue applicable à la Ligne.
        /// Quand une ligne est ajoutée à un bon, elle a la date du catalogue au moment de l'ajout.
        /// Quand un bon de commande est envoyé, ses lignes prennent la date du catalogue au moment de l'envoi.
        /// Quand une synthèse est enregistrée, sa date est fixée et les lignes du bon virtuel éventuel sont
        /// incorporées dans la synthèse avec la date de la synthèse et les lignes des autres bons avec la date qu'ils ont déjà.
        /// </summary>
        public DateTime Date { get; set; }

        public static IKeyDocSansType KeyDocSansType(IKeyLigneSansType keyLigneSansType)
        {
            return new KeyDocSansType { Id = keyLigneSansType.Id, No = keyLigneSansType.No };
        }
    }
}
