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
    public class FournisseurDIdentifiant : IFournisseurData, IRoleEtat
    {
        [JsonProperty]
        public string Nom { get; set; }
        [JsonProperty]
        public string Adresse { get; set; }
        [JsonProperty]
        public string Ville { get; set; }
        [JsonProperty]
        public string Siret { get; set; }
        [JsonProperty]
        public EtatRole Etat { get; set; }
        [JsonProperty]
        public DateTime Date0 { get; set; }
        [JsonProperty]
        public DateTime DateEtat { get; set; }

        public FournisseurDIdentifiant(Fournisseur fournisseur)
        {
            Fournisseur.CopieData(fournisseur, this);
            Fournisseur.FixeRoleEtat(fournisseur, this);
        }

        public FournisseurDIdentifiant(DemandeSite demande)
        {
            Fournisseur.CopieData(demande.Fournisseur, this);
        }

        public FournisseurDIdentifiant(Invitation invitation)
        {
            Fournisseur.CopieData(invitation.Fournisseur, this);
        }
    }
}
