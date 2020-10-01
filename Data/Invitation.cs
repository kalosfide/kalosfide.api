using KalosfideAPI.Data.Constantes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data
{
    public interface IInvitationKey
    {

        /// <summary>
        /// Email de l'utilisateur invité
        /// </summary>
        public string Email { get; set; }

        // key du site
        public string Uid { get; set; }
        public int Rno { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Invitation: IInvitationKey
    {
        // key

        /// <summary>
        /// Email de l'utilisateur invité
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Uid du site
        /// </summary>
        public string Uid { get; set; }

        /// <summary>
        /// Rno du site
        /// </summary>
        public int Rno { get; set; }

        public DateTime Date { get; set; }

        // data
        public string UidClient { get; set; }
        public int? RnoClient { get; set; }

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            EntityTypeBuilder<Invitation> entity = builder.Entity<Invitation>();

            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(LongueurMax.UId);
            entity.Property(e => e.Uid).IsRequired();
            entity.Property(e => e.Uid).HasMaxLength(LongueurMax.UId);
            entity.Property(e => e.Rno).IsRequired();
            entity.Property(e => e.UidClient).HasMaxLength(LongueurMax.UId);

            entity.HasKey(e => new
            {
                e.Email,
                e.Uid,
                e.Rno
            });
        }
    }
}
