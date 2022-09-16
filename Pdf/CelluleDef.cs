using MigraDoc.DocumentObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Pdf
{
    public class FormatDeCellule
    {
        public ParagraphAlignment Alignment { get; set; }

        public void AppliqueA(Paragraph paragraph)
        {
            paragraph.Format.Alignment = Alignment;
        }
    }
    public class CelluleDef
    {
        public int Index { get; set; }
        public int NbColonnes { get; set; }
        public ElémentDeParagraphe Contenu { get; set; }
        public FormatDeCellule Format { get; set; }
    }
}
