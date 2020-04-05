using System;
using System.ComponentModel.DataAnnotations;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using Microsoft.EntityFrameworkCore;

namespace KalosfideAPI.Data
{
    public class ArchiveRole : AKeyUidRno, IKeyArchive
    {
        // key
        [Required]
        [MaxLength(LongueurMax.UId)]
        public override string Uid { get; set; }
        [Required]
        public override int Rno { get; set; }
        [Required]
        public DateTime Date { get; set; }

        [StringLength(1)]
        public string Etat { get; set; }

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            var entité = builder.Entity<ArchiveRole>();

            entité.HasKey(donnée => new { donnée.Uid, donnée.Rno, donnée.Date });

            entité.HasIndex(donnée => new { donnée.Uid, donnée.Rno });

            entité.ToTable("ArchiveRoles");
        }
    }
}
