using System;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Roles;

namespace KalosfideAPI.Clients
{
    public class ClientVue : AKeyUidRno, IRoleData
    {
        public override string Uid { get; set; }
        public override int Rno { get; set; }
        public string Nom { get; set; }
        public string Adresse { get; set; }
        public string Ville { get; set; }
        public string Etat { get; set; }

        public static RoleVue RoleVue(ClientVue vue)
        {
            return new RoleVue
            {
                Uid = vue.Uid,
                Rno = vue.Rno,
                Nom = vue.Nom,
                Adresse = vue.Adresse
            };
        }
    }
    public class ClientVueAjoute : AKeyUidRno, IRoleData
    {
        /// <summary>
        /// Uid du site
        /// </summary>
        public override string Uid { get; set; }

        /// <summary>
        /// Rno du Site
        /// </summary>
        public override int Rno { get; set; }

        public string Nom { get; set; }
        public string Adresse { get; set; }
        public string Ville { get; set; }
        public string Etat { get; set; }
    }
    public class ClientEtatVue : AKeyUidRno, IRoleData
    {
        public override string Uid { get; set; }
        public override int Rno { get; set; }

        public string Nom { get; set; }
        public string Adresse { get; set; }

        public string Ville { get; set; }

        public string Etat { get; set; }

        public DateTime Date0 { get; set; }

        public DateTime DateEtat { get; set; }

        public string Email { get; set; }

        public bool AvecDocuments { get; set; }

    }
}