using KalosfideAPI.Data;
using KalosfideAPI.Pdf;
using MigraDoc.DocumentObjectModel;
using System.Collections.Generic;
using System.Globalization;

namespace KalosfideAPI.CLF
{
    public static class CLFPdfColonnes
    {
        static ColonneDef<CLFPdfLigne> Catégorie()
        {
            return new ColonneDef<CLFPdfLigne>
            {
                EnTête = delegate ()
                {
                    return new TexteDef("Catégorie");
                },
                CelluleDef = delegate (CLFPdfLigne ligne)
                    {
                        return new TexteDef(ligne.Catégorie);
                    }
            };
        }
        static ColonneDef<CLFPdfLigne> Produit()
        {
            return new ColonneDef<CLFPdfLigne>
            {
                EnTête = delegate ()
                {
                    return new TexteDef("Produit");
                },
                CelluleDef = delegate (CLFPdfLigne ligne)
                    {
                        return new TexteDef(ligne.Produit);
                    },
            };
        }
        static ColonneDef<CLFPdfLigne> Prix()
        {
            return new ColonneDef<CLFPdfLigne>
            {
                EnTête = delegate ()
                {
                    return new TexteDef("Prix");
                },
                CelluleDef = delegate (CLFPdfLigne ligne)
                    {
                        TexteDef prix = new TexteDef(ligne.TextePrix);
                        if (ligne.DateProduit.HasValue)
                        {
                            return new TextesDef(new List<TexteDef>
                            {
                                    prix,
                                    new TexteDef(string.Format(CultureInfo.CurrentCulture, "{0:d}", ligne.DateProduit))
                            });

                        }
                        else
                        {
                            return prix;
                        }
                    },
                FormatCellule = new FormatDeCellule { Alignment = ParagraphAlignment.Right }
            };
        }
        static ColonneDef<CLFPdfLigne> Quantité()
        {
            return new ColonneDef<CLFPdfLigne>
            {
                EnTête = delegate ()
                {
                    return new TexteDef("Quantité");
                },
                CelluleDef = delegate (CLFPdfLigne ligne)
                    {
                        TexteDef quantitéDef = new TexteDef(ligne.TexteQuantité);
                        if (ligne.TexteUnités != null)
                        {
                            return new TextesDef(new List<TexteDef>
                            {
                                    quantitéDef,
                                    new TexteDef(ligne.TexteUnités)
                            });

                        }
                        else
                        {
                            return quantitéDef;
                        }
                    },
                FormatCellule = new FormatDeCellule { Alignment = ParagraphAlignment.Right }
            };
        }
        static ColonneDef<CLFPdfLigne> Coût()
        {
            return new ColonneDef<CLFPdfLigne>
            {
                EnTête = delegate ()
                {
                    return new TexteDef("Coût");
                },
                CelluleDef = delegate (CLFPdfLigne ligne)
                    {
                        return new TexteDef(ligne.TexteCoût);
                    },
                FormatCellule = new FormatDeCellule { Alignment = ParagraphAlignment.Right },
                Bilan = new BilanTable<CLFPdfLigne>
                {
                    Titre = new TexteDef("Total"),
                    Valeur = delegate (List<CLFPdfLigne> lignes)
                    {
                        decimal total = 0;
                        foreach (CLFPdfLigne ligne in lignes)
                        {
                            total += ligne.Coût;
                        }
                        return new TexteDef(string.Format(CultureInfo.CurrentCulture, "{0:C2}", total));
                    }
                }
            };
        }
    public static List<ColonneDef<CLFPdfLigne>> Defs()
        {
            return new List<ColonneDef<CLFPdfLigne>>
            {
                Catégorie(),
                Produit(),
                Prix(),
                Quantité(),
                Coût()
            };
        }
    }
}
