using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;

namespace KalosfideAPI.Pdf
{
    public class TextesDef : ElémentDeParagraphe
    {
        public List<TexteDef> Defs { get; set; }

        public TextesDef(List<TexteDef> defs)
        {
            Defs = defs;
        }

        public void AjouteA(Paragraph paragraphe)
        {
            foreach (TexteDef def in Defs)
            {
                def.AjouteA(paragraphe);
            }
        }
    }
}
