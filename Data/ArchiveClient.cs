using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KalosfideAPI.Data
{
    public class ArchiveClient : AKeyUidRno, IKeyArchive
    {
        // key
        [Required]
        [MaxLength(LongueurMax.UId)]
        public override string Uid { get; set; }
        [Required]
        public override int Rno { get; set; }
        [Required]
        public DateTime Date { get; set; }

        [MaxLength(200)]
        public string Nom { get; set; }
        [MaxLength(500)]
        public string Adresse { get; set; }

        // création
        public static void CréeTable(ModelBuilder builder)
        {
            var entité = builder.Entity<ArchiveClient>();

            entité.HasKey(donnée => new { donnée.Uid, donnée.Rno, donnée.Date });

            entité.HasIndex(donnée => new { donnée.Uid, donnée.Rno });

            entité.ToTable("ArchiveClients");
        }

    }

}