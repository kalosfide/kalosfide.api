using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KalosfideAPI.Roles
{
    public interface IRoleEtat
    {
        /// <summary>
        /// Une des valeurs de TypeEtatRole.
        /// </summary>
        string Etat { get; set; }

        /// <summary>
        /// Date de création.
        /// </summary>
        DateTime Date0 { get; set; }

        /// <summary>
        /// Date du dernier changement d'état.
        /// </summary>
        DateTime DateEtat { get; set; }

    }

    public class RoleEtat: IRoleEtat
    {
        /// <summary>
        /// Une des valeurs de TypeEtatRole.
        /// </summary>
        public string Etat { get; set; }

        /// <summary>
        /// Date de création.
        /// </summary>
        public DateTime Date0 { get; set; }

        /// <summary>
        /// Date du dernier changement d'état.
        /// </summary>
        public DateTime DateEtat { get; set; }

        /// <summary>
        /// RoleEtat d'un role.
        /// </summary>
        /// <param name="role">Role qui inclut ses archives</param>
        public static void FixeEtat(Role role, IRoleEtat roleEtat)
        {
            IEnumerable<ArchiveRole> archives = role.Archives.Where(a => a.Etat != null).OrderBy(a => a.Date);
            roleEtat.Etat = role.Etat;
            roleEtat.Date0 = archives.First().Date;
            roleEtat.DateEtat = archives.Last().Date;
        }

        /// <summary>
        /// RoleEtat ne contenant que la date de l'état.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static RoleEtat DeDate(DateTime date)
        {
            return new RoleEtat
            {
                DateEtat = date
            };
        }
    }

    public class RoleVue : AKeyUidRno, IRoleData
    {
        public override string Uid { get; set; }
        public override int Rno { get; set; }

        // données
        public string SiteUid { get; set; }
        public int SiteRno { get; set; }

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

        // calculés
        public string Etat { get; set; }
        public string Url { get; set; }
    }
    public class RoleData : IRoleData
    {

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

        public static RoleData Ancien(Role role, DateTime date)
        {
            var archives = role.Archives.OrderBy(a => a.Date);
            if (archives.Last().Date <= date)
            {
                return null;
            }
            RoleData ancien = new RoleData();
            archives.ToList().ForEach(archive =>
            {
                if (archive.Nom != null) { ancien.Nom = archive.Nom; }
                if (archive.Adresse != null) { ancien.Adresse = archive.Adresse; }
                if (archive.Ville != null) { ancien.Ville = archive.Ville; }
            });
            return ancien;
        }
    }
}
