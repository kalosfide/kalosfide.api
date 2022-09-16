using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Pdf
{
    public class TableDef<T>
    {
        public List<ColonneDef<T>> ColonneDefs { get; set; }

        public NoLigneDef NoLigneDef { get; set; }

        public FormatDeCellule FormatEnTête { get; set; }

        const double LargeurNoParDéfaut = 0.8;
        const string EnTêteNoParDéfaut = "#";
        double? LargeurNo { get; set; }

        Table Table { get; set; }

        Document Document => Table.Document;

        List<CelluleBilanDef<T>> LigneBilanDefs { get; set; }

        private void PrépareLargeurs()
        {
            List<ColonneDef<T>> colonneDefsSansLargeur = ColonneDefs.Where(def => def.Largeur == 0).ToList();
            int nbSansLargeur = colonneDefsSansLargeur.Count;
            if (nbSansLargeur > 0)
            {
                PageSetup pageSetup = Document.DefaultPageSetup;
                double largeurTotale = pageSetup.PageWidth.Centimeter - pageSetup.LeftMargin.Centimeter - pageSetup.RightMargin.Centimeter;
                double largeurDéfinie = LargeurNo ?? 0;
                foreach (ColonneDef<T> colonneDef in ColonneDefs)
                {
                    largeurDéfinie += colonneDef.Largeur;
                }
                double largeurRestantePartagée = (largeurTotale - largeurDéfinie) / nbSansLargeur;
                foreach (ColonneDef<T> colonneDef in ColonneDefs)
                {
                    colonneDef.Largeur = largeurRestantePartagée;
                }
            }
        }

        private void PrépareBilan()
        {
            if (ColonneDefs.All(def => def.Bilan == null))
            {
                return;
            }
            List<BilanTable<T>> bilanDefs = ColonneDefs.Select(colonneDef => colonneDef.Bilan).ToList();
            if (NoLigneDef != null)
            {
                bilanDefs.Insert(0, null);
            }
            LigneBilanDefs = new List<CelluleBilanDef<T>>();
            int indexColonneAvecBilanPrécédente = -1;
            for (int i = 0; i < bilanDefs.Count; i++)
            {
                BilanTable<T> bilanTable = bilanDefs[i];
                if (bilanTable != null)
                {
                    int indexColonneAvantBilan = i - 1;
                    if (indexColonneAvantBilan >= 0 && indexColonneAvantBilan > indexColonneAvecBilanPrécédente)
                    {
                        // il y a une colonne sans bilan avant la colonne avec bilan
                        // la cellule suivant la dernière cellule avec bilan doit s'étendre jusquà cette colonne
                        LigneBilanDefs.Add(new CelluleBilanDef<T>
                        {
                            Index = indexColonneAvecBilanPrécédente + 1,
                            Titre = bilanTable.Titre,
                            NbColonnes = indexColonneAvantBilan - indexColonneAvecBilanPrécédente - 1
                        });
                    }
                    LigneBilanDefs.Add(new CelluleBilanDef<T>
                    {
                        Index = i,
                        Valeur = bilanTable.Valeur
                    });
                    indexColonneAvecBilanPrécédente = i;
                }
            }
            if (indexColonneAvecBilanPrécédente + 1 < ColonneDefs.Count)
            {
                // il y a des cellules vides après la dernière cellule avec bilan
                LigneBilanDefs.Add(new CelluleBilanDef<T>
                {
                    Index = indexColonneAvecBilanPrécédente + 1,
                    NbColonnes = ColonneDefs.Count - indexColonneAvecBilanPrécédente - 1
                });
            }
        }

        public void AjouteA(Document document)
        {
            Table = document.LastSection.AddTable();
            Table.Style = NomStyles.Table;
            Table.Borders.Color = Colors.DarkGray;
            Table.Borders.Width = 0.25;
            Table.Borders.Left.Width = 0.5;
            Table.Borders.Right.Width = 0.5;

            if (NoLigneDef != null)
            {
                LargeurNo = NoLigneDef.Largeur ?? LargeurNoParDéfaut;
                Table.AddColumn(Unit.FromCentimeter(LargeurNo.Value));
            }
            PrépareLargeurs();
            foreach (ColonneDef<T> colonneDef in ColonneDefs)
            {
                Table.AddColumn(Unit.FromCentimeter(colonneDef.Largeur));
            }
        }

        public void Remplit(List<T> items)
        {
            AjouteEnTête();
            AjouteLignes(items);
            PrépareBilan();
            if (LigneBilanDefs != null)
            {
                AjouteBilan(items);
            }
        }

        private Row AjouteLigne(List<CelluleDef> celluleDefs)
        {
            Row row = Table.AddRow();
            foreach (CelluleDef def in celluleDefs)
            {
                Cell cell = row.Cells[def.Index];
                if (def.NbColonnes != 0)
                {
                    cell.MergeRight = def.NbColonnes;
                }
                Paragraph paragraph = cell.AddParagraph();
                if (def.Contenu != null)
                {
                    def.Contenu.AjouteA(paragraph);
                }
                if (def.Format != null)
                {
                    def.Format.AppliqueA(paragraph);
                }
            }
            return row;
        }

        public void AjouteEnTête()
        {
            List<CelluleDef> defs = new List<CelluleDef>();
            int index0 = 0;
            if (NoLigneDef != null)
            {
                defs.Add(new CelluleDef
                {
                    Index = 0,
                    Contenu = new TexteDef(NoLigneDef.EnTête != null ? NoLigneDef.EnTête : EnTêteNoParDéfaut),
                    Format = NoLigneDef.FormatEntête ?? FormatEnTête
                });
                index0 = 1;
            }
            for (int index = 0; index < ColonneDefs.Count; index++)
            {
                ColonneDef<T> colonneDef = ColonneDefs[index];
                defs.Add(new CelluleDef
                {
                    Index = index + index0,
                    Contenu = colonneDef.EnTête(),
                    Format = colonneDef.FormatEnTête ?? FormatEnTête
                });
            }
            Row ligne = AjouteLigne(defs);
            Table.SetEdge(0, ligne.Index, ColonneDefs.Count, 1, Edge.Box, BorderStyle.Single, 0.75, Color.Empty);
            ligne.HeadingFormat = true;
            ligne.Format.Font.Bold = true;
            ligne.Shading.Color = Colors.LightGray;
        }

        public void AjouteLignes(List<T> items)
        {
            int noLigne = 1;
            foreach (T item in items)
            {
                List<CelluleDef> defs = new List<CelluleDef>();
                int index0 = 0;
                if (NoLigneDef != null)
                {
                    defs.Add(new CelluleDef
                    {
                        Index = 0,
                        Contenu = new TexteDef(noLigne.ToString()),
                        Format = NoLigneDef.FormatNo
                    });
                    index0 = 1;
                    noLigne++;
                }
                for (int index = 0; index < ColonneDefs.Count; index++)
                {
                    ColonneDef<T> colonneDef = ColonneDefs[index];
                    defs.Add(new CelluleDef
                    {
                        Index = index + index0,
                        Contenu = colonneDef.CelluleDef(item),
                        Format = colonneDef.FormatCellule
                    });
                }
                AjouteLigne(defs);
            }
        }

        public void AjouteBilan(List<T> items)
        {
            List<CelluleDef> defs = LigneBilanDefs.Select(bilanDef => bilanDef.CelluleDef(items)).ToList();
            Row ligne = AjouteLigne(defs);
            Table.SetEdge(0, ligne.Index, ColonneDefs.Count, 1, Edge.Box, BorderStyle.Single, 0.75, Color.Empty);
            ligne.HeadingFormat = true;
            ligne.Format.Font.Bold = true;
            ligne.Shading.Color = Colors.LightGray;
        }
    }
}
