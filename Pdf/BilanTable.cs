using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;

namespace KalosfideAPI.Pdf
{
    public class BilanTable<T>
    {
        public ElémentDeParagraphe Titre { get; set; }
        public Func<List<T>, ElémentDeParagraphe> Valeur { get; set; }
    }

    public class CelluleBilanDef<T>
    {
        public int Index { get; set; }
        public int NbColonnes { get; set; }
        /// <summary>
        /// Titre du bilan. Si présent, Valeur est ignoré.
        /// </summary>
        public ElémentDeParagraphe Titre { get; set; }
        /// <summary>
        /// Fonction de calcul du bilan d'une liste. Doit être présent si Titre est absent.
        /// </summary>
        public Func<List<T>, ElémentDeParagraphe> Valeur { get; set; }

        public CelluleDef CelluleDef(List<T> items)
        {
            if (Titre != null)
            {
                return new CelluleDef
                {
                    Index = Index,
                    NbColonnes = NbColonnes,
                    Contenu = Titre,
                    Format = new FormatDeCellule { Alignment = ParagraphAlignment.Right }
                };
            }
            return new CelluleDef
            {
                Index = Index,
                NbColonnes = NbColonnes,
                Contenu = Valeur(items),
                Format = new FormatDeCellule { Alignment = ParagraphAlignment.Right }
            };
        }

    }
}
