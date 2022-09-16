using KalosfideAPI.Data;
using System;
using System.Collections.Generic;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using KalosfideAPI.Pdf;
using System.Globalization;

namespace KalosfideAPI.CLF
{
    public class CLFPdfAEnvoyer
    {
        public Client Client { get; set; }
        public string Pdf { get; set; }
        public DateTime Date { get; set; }

        /// <summary>
        /// Liste des dates du dernier téléchargement du document par l'utilisateur qui demande le document.
        /// </summary>
        public List<DateTime> Téléchargé { get; set; }

        /// <summary>
        /// Nombre de fois que l'utilisateur a téléchargé un document.
        /// </summary>
        public int Téléchargements { get; set; }
    }
    public class CLFPdfDoc
    {
        public Client Client { get; set; }

        public TypeCLF Type { get; set; }

        public uint No { get; set; }

        public DateTime Date { get; set; }

        /// <summary>
        /// Liste des dates du dernier téléchargement du document par l'utilisateur qui demande le document.
        /// </summary>
        public List<DateTime> Téléchargé { get; set; }

        public List<CLFPdfLigne> Lignes { get; set; }

        /// <summary>
        /// Nombre de fois que l'utilisateur a téléchargé un document.
        /// </summary>
        public int Téléchargements { get; set; }

        private void AjouteParagrapheRole(Section section, RoleData role, string style)
        {
            Paragraph paragraph = section.AddParagraph();
            paragraph.AddText(role.Nom);
            paragraph.AddLineBreak();
            paragraph.AddText(role.Adresse);
            paragraph.AddLineBreak();
            paragraph.AddText(role.Ville);
            paragraph.Style = style;
        }

        private Pdf.Pdf CréePdf(string auteur, string nomFichier)
        {
            string date = Date == null ? "xx-xx-xxxx" : string.Format(CultureInfo.CurrentCulture, "{0:d}", Date);
            string nomDocument = Type == TypeCLF.Commande ? "Bon de commande" : Type == TypeCLF.Livraison ? "Bon de livraison" : "Facture";
            string titreDocument = nomDocument + " n° " + No;
            string titreAvecClientEtDate = Type == TypeCLF.Commande
                ? string.Format("{0} - {1} ({2:d})", Client.Nom, titreDocument, date)
                : string.Format("{0} ({2:d}) - {1}", titreDocument, date, Client.Nom);
            RoleData expéditeur = new RoleData();
            RoleData destinataire = null;
            if (Type == TypeCLF.Commande)
            {
                Role.CopieData(Client, expéditeur);
            }
            else
            {
                Role.CopieData(Client.Site.Fournisseur, expéditeur);
                destinataire = new RoleData();
                Role.CopieData(Client, destinataire);
            }

            Pdf.Pdf pdf = new Pdf.Pdf();
            pdf.Info(new DocumentInfo
            {
                Author = auteur + " (avec Kalosfide)",
                Title = nomFichier,
            });
            Section section = pdf.Document.LastSection;

            // En-têtes et pieds de page 
            HeaderFooter header = section.Headers.FirstPage;
            Paragraph paragraph = header.AddParagraph(Client.Site.Titre);
            paragraph.Style = NomStyles.TitreSite;

            header = section.Headers.Primary;
            paragraph = header.AddParagraph(titreAvecClientEtDate);
            paragraph.Format.Font.Name = "Times New Roman";
            paragraph.Format.Font.Size = 9;

            header = section.Footers.Primary;
            paragraph = header.AddParagraph();
            paragraph.AddTab();
            paragraph.AddPageField();
            paragraph.AddText("/");
            paragraph.AddNumPagesField();
            paragraph.Style = StyleNames.Footer;

            // Ajoute l'expéditeur
            AjouteParagrapheRole(section, expéditeur, NomStyles.Expéditeur);

            // Ajoute le destinataire si ce n'est pas une commande
            if (destinataire != null)
            {
                AjouteParagrapheRole(section, destinataire, NomStyles.Destinataire);
            }

            // Ajoute le titre et la Date
            paragraph = section.AddParagraph();
            paragraph.AddText(titreDocument);
            paragraph.AddTab();
            paragraph.AddText(date);
            paragraph.Style = NomStyles.TitreDate;

            // Crée la table
            TableDef<CLFPdfLigne> def = new TableDef<CLFPdfLigne>
            {
                FormatEnTête = new FormatDeCellule { Alignment = ParagraphAlignment.Center },
                NoLigneDef = new NoLigneDef
                {
                    FormatNo = new FormatDeCellule { Alignment = ParagraphAlignment.Center }
                },
                ColonneDefs = CLFPdfColonnes.Defs()
            }; 
            pdf.AjouteTable(def, Lignes);

            Mesure(pdf);

            return pdf;
        }

        public void Mesure(Pdf.Pdf pdf)
        {
            DocumentRenderer renderer = new DocumentRenderer(pdf.Document);
            renderer.PrepareDocument();
            if (renderer.FormattedDocument.PageCount == 1)
            {
                return;
            }
            List<Unit> Hauts = new List<Unit>();
            for (int i = 1; i <= renderer.FormattedDocument.PageCount; i++)
            {
                RenderInfo[] infos = renderer.GetRenderInfoFromPage(i);
                for (int j = 0; j < infos.Length; j++)
                {
                    if (infos[j].GetType() != typeof(TableRenderInfo))
                    {
                        continue;
                    }
                    TableRenderInfo tableRenderInfo = (TableRenderInfo)infos[j];
                    Hauts.Add(new Unit(tableRenderInfo.LayoutInfo.ContentArea.Y));
                }

                var objets = renderer.GetDocumentObjectsFromPage(i);
            }
        }

        public CLFPdfAEnvoyer CLFPdfAEnvoyer(bool utilisateurEstLeClient)
        {
            string auteur = Type == TypeCLF.Commande ? Client.Nom : Client.Site.Fournisseur.Nom;
            string nomFichier = utilisateurEstLeClient
                ? Client.NomFichier(Client, Type, No)
                : Fournisseur.NomFichier(Client.Site.Fournisseur, Client, Type, No);
            
            Pdf.Pdf pdf = CréePdf(auteur, nomFichier);
            Client client = new Client { Id = Client.Id };
            Role.CopieData(Client, client);
            return new CLFPdfAEnvoyer
            {
                Client = client,
                Pdf = Convert.ToBase64String(pdf.DocumentToByteArray()),
                Date = Date,
                Téléchargé = Téléchargé,
                Téléchargements = Téléchargements
            };
        }
    }
}
