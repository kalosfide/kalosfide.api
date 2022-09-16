using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MigraDoc;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using PdfSharp.Pdf.IO;

namespace KalosfideAPI.Pdf
{
    public static class NomStyles
    {
        public static string Normal = StyleNames.Normal;
        public static string TitreSite = "TitreSite";
        public static string Expéditeur = "Expéditeur";
        public static string Destinataire = "Destinataire";
        public static string TitreDate = "TitreDate";
        public static string NomDocument = "NomDocument";
        public static string Table = "Table";
    }
    public class Pdf
    {
        public Document Document { get; private set; }

        public Pdf()
        {
            Document = new Document();
            Section section = Document.AddSection();
            section.PageSetup.DifferentFirstPageHeaderFooter = true;
            section.PageSetup.StartingNumber = 1;
            DefineStyles();
        }

        public static string SimplePdfBase64String()
        {
            Document document = new Document();
            Section section = document.AddSection();
            Paragraph paragraph = section.AddParagraph("Simple PDF", StyleNames.Heading1);
            section.AddParagraph("A simple PDF file to use as exemple.");
            MigraDoc.Rendering.PdfDocumentRenderer renderer = new MigraDoc.Rendering.PdfDocumentRenderer
            {
                Document = document
            };
            renderer.RenderDocument();
            byte[] fileContents = null;
            using (MemoryStream stream = new MemoryStream())
            {
                renderer.PdfDocument.Save(stream, true);
                fileContents = stream.ToArray();
            }
            return Convert.ToBase64String(fileContents);
        }

        public void Info(DocumentInfo info)
        {
            if (info.Title != null)
            {
                Document.Info.Title = info.Title;
            }
            if (info.Author != null)
            {
                Document.Info.Author = info.Author;
            }
            if (info.Subject != null)
            {
                Document.Info.Subject = info.Subject;
            }
            if (info.Comment != null)
            {
                Document.Info.Comment = info.Comment;
            }
        }

        /// <summary>
        /// Defines the styles used in the document.
        /// </summary>
        public void DefineStyles()
        {
            // Get the predefined style Normal.
            Style style = Document.Styles[StyleNames.Normal];
            // Because all styles are derived from Normal, the next line changes the
            // font of the whole document. Or, more exactly, it changes the font of
            // all styles and paragraphs that do not redefine the font.
            style.Font.Name = "Verdana";

            // Heading1 to Heading9 are predefined styles with an outline level. An outline level
            // other than OutlineLevel.BodyText automatically creates the outline (or bookmarks)
            // in PDF.

            style = Document.Styles[StyleNames.Heading1];
            style.Font.Name = "Tahoma";
            style.Font.Size = 14;
            style.Font.Bold = true;
            style.Font.Color = Colors.DarkBlue;
            style.ParagraphFormat.PageBreakBefore = true;
            style.ParagraphFormat.SpaceAfter = 6;

            style = Document.Styles[StyleNames.Heading2];
            style.Font.Size = 12;
            style.Font.Bold = true;
            style.ParagraphFormat.PageBreakBefore = false;
            style.ParagraphFormat.SpaceBefore = 6;
            style.ParagraphFormat.SpaceAfter = 6;
            style = Document.Styles[StyleNames.Heading3];
            style.Font.Size = 10;
            style.Font.Bold = true;
            style.Font.Italic = true;
            style.ParagraphFormat.SpaceBefore = 6;
            style.ParagraphFormat.SpaceAfter = 3;

            style = Document.Styles[StyleNames.Header];
            style.ParagraphFormat.AddTabStop("16cm", TabAlignment.Right);

            style = Document.Styles[StyleNames.Footer];
            style.ParagraphFormat.AddTabStop("8cm", TabAlignment.Center);

            // Create a new style called TitreSite based on style Normal
            style = Document.Styles.AddStyle(NomStyles.TitreSite, "Normal");
            style.Font.Name = "Tahoma";
            style.Font.Size = 12;
            style.Font.Bold = true;
            // Create a new style called  based on style Normal
            style = Document.Styles.AddStyle(NomStyles.Expéditeur, "Normal");
            style.ParagraphFormat.SpaceBefore = 6;
            style.ParagraphFormat.SpaceAfter = 3;
            // Create a new style called  based on style Normal
            style = Document.Styles.AddStyle(NomStyles.Destinataire, "Normal");
            style.ParagraphFormat.SpaceBefore = 6;
            style.ParagraphFormat.SpaceAfter = 3;
            style.ParagraphFormat.LeftIndent = "9cm";
            // Create a new style called TitreDate based on style Normal
            style = Document.Styles.AddStyle(NomStyles.TitreDate, "Normal");
            style.Font.Name = "Tahoma";
            style.Font.Size = 14;
            style.Font.Bold = true;
            style.ParagraphFormat.AddTabStop("16cm", TabAlignment.Right);
            // Create a new style called Table based on style Normal
            style = Document.Styles.AddStyle(NomStyles.Table, "Normal");
            style.Font.Name = "Times New Roman";
            style.Font.Size = 9;

        }

        public void AjouteTable<T>(TableDef<T> tableDef, List<T> items)
        {
            tableDef.AjouteA(Document);
            tableDef.Remplit(items);
        }
        public byte[] DocumentToByteArray()
        {
            MigraDoc.Rendering.PdfDocumentRenderer renderer = new MigraDoc.Rendering.PdfDocumentRenderer
            {
                Document = Document
            };
            renderer.RenderDocument();

            var securitySettings = renderer.PdfDocument.SecuritySettings;
            securitySettings.OwnerPassword = "OwnerPassword";
            securitySettings.PermitExtractContent = false;
            securitySettings.PermitFormsFill = false;
            securitySettings.PermitPrint = false;
            securitySettings.PermitAnnotations = false;
            securitySettings.PermitAssembleDocument = false;
            securitySettings.PermitModifyDocument = false;

            string testPath = "C:\\Users\\François Bonnefoi\\Documents\\test.pdf";
            renderer.PdfDocument.Save(testPath);

            byte[] fileContents = null;
            using (MemoryStream stream = new MemoryStream())
            {
                renderer.PdfDocument.Save(stream, true);
                fileContents = stream.ToArray();
            }
            return fileContents;
        }

        public static Paragraph Paragraphe(string texte)
        {
            Paragraph paragraphe = new Paragraph();
            paragraphe.AddFormattedText(texte);
            return paragraphe;
        }
    }
}
