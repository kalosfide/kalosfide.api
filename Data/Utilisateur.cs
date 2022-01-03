using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data
{
    public enum EtatUtilisateur
    {
        Nouveau,
        Actif,
        Inactif,
        Banni
    }

    public class Utilisateur : IdentityUser
    {
        // données
        public EtatUtilisateur Etat { get; set; }

        /// <summary>
        /// Vaut 0 si l'utilisateur ne s'est jamais connecté.
        /// Augmente de 1 à chaque connection.
        /// Est changé en son opposé à chaque déconnection. 
        /// </summary>
        public int SessionId { get; set; }

        // navigation
        public virtual ICollection<Fournisseur> Fournisseurs { get; set; }
        public virtual ICollection<Client> Clients { get; set; }
        public virtual ICollection<ArchiveUtilisateur> Archives { get; set; }

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            var entité = builder.Entity<Utilisateur>();

            entité.Property(donnée => donnée.Etat).HasDefaultValue(EtatUtilisateur.Nouveau);
            entité.Property(donnée => donnée.SessionId).HasDefaultValue(0);

            entité.ToTable("Utilisateur");
        }

        // utiles
        public static bool EstUsager(Utilisateur utilisateur, uint idSite)
        {
            return utilisateur.Fournisseurs.Where(f => f.Id == idSite).Any()
                || utilisateur.Clients.Where(c => c.SiteId == idSite).Any();
        }
    }
}
