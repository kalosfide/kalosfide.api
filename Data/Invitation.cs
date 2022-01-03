using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data
{
    public interface IInvitationKey
    {
        /// <summary>
        /// Id du site
        /// </summary>
        uint Id { get; set; }

        /// <summary>
        /// Email de l'utilisateur invité
        /// </summary>
        string Email { get; set; }
    }
    public class InvitationKey : IInvitationKey
    {

        /// <summary>
        /// Email de l'utilisateur invité
        /// </summary>
        public string Email { get; set; }

        // Id du site
        public uint Id { get; set; }
    }

    public interface IInvitationData
    {
        string Email { get; set; }
        DateTime Date { get; set; }
        uint? ClientId { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Invitation: IInvitationKey, IInvitationData
    {
        // key
        /// <summary>
        /// Id du site
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        /// Email de l'utilisateur invité
        /// </summary>
        [EmailAddress]
        public string Email { get; set; }

        public DateTime Date { get; set; }

        // data
        public uint? ClientId { get; set; }

        // navigation
        public virtual Fournisseur Fournisseur { get; set; }
        public virtual Client Client { get; set; }

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            EntityTypeBuilder<Invitation> entity = builder.Entity<Invitation>();

            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(LongueurMax.Email);
            entity.Property(e => e.Id).IsRequired();

            entity.HasKey(e => new
            {
                e.Email,
                e.Id
            });

            entity.HasOne(i => i.Fournisseur).WithMany(f => f.Invitations).HasPrincipalKey(f => f.Id).HasForeignKey(i => i.Id);
            entity.HasOne(i => i.Client).WithOne().HasForeignKey<Invitation>(i => i.ClientId).OnDelete(DeleteBehavior.ClientSetNull);
        }
    }
}
