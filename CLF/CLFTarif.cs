using KalosfideAPI.Catalogues;
using KalosfideAPI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.CLF
{
    public class LigneCLFTarif
    {
        public LigneCLF Ligne { get; set; }
        public ArchiveProduit ArchiveProduit { get; set; }
        public ArchiveCatégorie ArchiveCatégorie { get; set; }
    }
    public class DocCLFLignesCLFTarifs
    {
        public DocCLF DocCLF { get; set; }
        public IEnumerable<LigneCLFTarif> LigneCLFTarifs { get; set; }
    }
    public class CLFTarif
    {
        public CLFLigneData Ligne { get; set; }
        public ArchiveProduit ArchiveProduit { get; set; }
        public ArchiveCatégorie ArchiveCatégorie { get; set; }
    }

    public class DocCLFTarif
    {
        public DocCLF DocCLF { get; set; }
        public IEnumerable<LigneCLF> Lignes { get; set; }
        public IEnumerable<ArchiveProduit> ArchivesProduit { get; set; }
        public IEnumerable<ArchiveCatégorie> ArchivesCatégorie { get; set; }
    }
}
