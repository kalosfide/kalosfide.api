using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;

namespace KalosfideAPI.Pdf
{
    public class TexteDef: ElémentDeParagraphe
    {
        public string Texte { get; private set; }

        public TextFormat Format { get; set; }
        public string Style { get; private set; }

        public double? NbPoints { get; set; }
        public Color Couleur { get; set; }
        public bool SuiviDeSaut { get; set; }

        public TexteDef(string texte)
        {
            Texte = texte;
        }

        public TexteDef(string texte, string style)
        {
            Texte = texte;
            Style = style;
        }

        public void AjouteA(Paragraph paragraphe)
        {
            if (Style != null)
            {
                paragraphe.AddFormattedText(Texte, Style);
            }
            else
            {
                FormattedText formattedText = paragraphe.AddFormattedText(Texte, Format);
                if (NbPoints != null)
                {
                    formattedText.Size = NbPoints.Value;
                }
                if (Couleur != null)
                {
                    formattedText.Color = Couleur;
                }
                if (SuiviDeSaut == true)
                {
                    paragraphe.AddLineBreak();
                }
            }
        }
    }
}
