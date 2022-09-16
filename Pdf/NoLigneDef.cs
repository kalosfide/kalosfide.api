using MigraDoc.DocumentObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Pdf
{
    public class NoLigneDef
    {
        public double? Largeur { get; set; } 
        public string EnTête { get; set; }
        public FormatDeCellule FormatEntête { get; set; }
        public FormatDeCellule FormatNo { get; set; }
    }
}
