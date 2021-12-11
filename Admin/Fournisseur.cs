using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Roles;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Admin
{

    [JsonObject(MemberSerialization.OptIn)]
    public class Fournisseur : AKeyUidRno, IRoleData, IRoleEtat, ISiteDef
    {
        [JsonProperty]
        public override string Uid { get; set; }
        [JsonProperty]
        public override int Rno { get; set; }
        [JsonProperty]
        public string Nom { get; set; }
        [JsonProperty]
        public string Adresse { get; set; }
        [JsonProperty]
        public string Ville { get; set; }

        /// <summary>
        /// Une des valeurs de TypeEtatRole.
        /// </summary>
        [JsonProperty]
        public string Etat { get; set; }

        /// <summary>
        /// Date de création.
        /// </summary>
        [JsonProperty]
        public DateTime Date0 { get; set; }

        /// <summary>
        /// Date du dernier changement d'état.
        /// </summary>
        [JsonProperty]
        public DateTime DateEtat { get; set; }

        /// <summary>
        /// Url du Site
        /// </summary>
        [JsonProperty]
        public string Url { get; set; }

        /// <summary>
        /// Titre du Site
        /// </summary>
        [JsonProperty]
        public string Titre { get; set; }

        /// <summary>
        /// Email de l'Utilisateur
        /// </summary>
        [JsonProperty]
        public string Email { get; set; }

        public Fournisseur(Role role)
        {
            CopieKey(role);
            Role.CopieDef(role, this);
            RoleEtat.FixeEtat(role, this);
            Site.CopieDef(role.Site, this);
            Email = role.Utilisateur.ApplicationUser.Email;
        }
    }

}
