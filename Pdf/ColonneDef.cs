using MigraDoc.DocumentObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Pdf
{
    public class ColonneDef<T>
    {
        public Func<ElémentDeParagraphe> EnTête { get; set; }

        public FormatDeCellule FormatEnTête { get; set; }

        public Func<T, ElémentDeParagraphe> CelluleDef { get; set; }

        public FormatDeCellule FormatCellule { get; set; }

        public double Largeur { get; set; }

        public BilanTable<T> Bilan { get; set; }

    }
}
