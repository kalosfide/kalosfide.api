using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KalosfideAPI.Data
{
    public class DemandeSite
    {
        // key
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Id du Fournisseur et du Site créés à l'enregistrement de la demande.
        /// </summary>
        [Required]
        public uint Id { get; set; }

        /// <summary>
        /// Date de la demande.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Date d'envoi du message d'activation.
        /// </summary>
        public DateTime? Envoi { get; set; }

        // navigation
        [JsonIgnore]
        public virtual Fournisseur Fournisseur { get; set; }

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            var entité = builder.Entity<DemandeSite>();

            entité.HasKey(donnée => donnée.Email);

            entité.HasOne(donnée => donnée.Fournisseur).WithOne().HasForeignKey<DemandeSite>(i => i.Id).OnDelete(DeleteBehavior.Cascade);

            entité.ToTable("DemandesSite");
        }
    }
}
