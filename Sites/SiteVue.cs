using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Newtonsoft.Json;

namespace KalosfideAPI.Sites
{
    public class SiteVue : AKeyUidRno
    {
        public override string Uid { get; set; }
        public override int Rno { get; set; }
        public string Url { get; set; }
        public string Titre { get; set; }

        public bool? Ouvert { get; set; }

        /// <summary>
        /// nb de produits disponibles
        /// </summary>
        public int? NbProduits { get; set; }

        /// <summary>
        /// nombre de clients d'état Nouveau ou Actif
        /// </summary>
        public int? NbClients { get; set; }

        public void Copie(Site site)
        {
            Uid = site.Uid;
            Rno = site.Rno;
            Url = site.Url;
            Titre = site.Titre;
            Ouvert = site.Ouvert;
        }
    }
}