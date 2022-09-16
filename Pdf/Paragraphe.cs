using MigraDoc.DocumentObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Pdf
{
    public class Paragraphe
    {
        public Paragraph Paragraph { get; private set; }
        public Paragraphe(string texte)
        {
            Paragraph = new Paragraph();
            Paragraph.AddFormattedText(texte);
        }
    }
}
