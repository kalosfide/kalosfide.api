using KalosfideAPI.CLF;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KalosfideAPI.Sécurité.Identifiants
{

    [JsonObject(MemberSerialization.OptIn)]
    public class InvitationDIdentifiant: ISiteData
    {
        [JsonProperty]
        public string Url { get; set; }
        [JsonProperty]
        public string Titre { get; set; }
        /// <summary>
        /// Date de la demande.
        /// </summary>
        [JsonProperty]
        public DateTime Date { get; set; }

        /// <summary>
        /// Fournisseur du site
        /// </summary>
        public FournisseurDIdentifiant Fournisseur { get; set; }

        public InvitationDIdentifiant(Invitation invitation)
        {
            Site.CopieData(invitation.Fournisseur.Site, this);
            Fournisseur = new FournisseurDIdentifiant(invitation);
            Date = invitation.Date;
        }

    }
}
