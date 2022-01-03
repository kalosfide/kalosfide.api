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
    public class SiteAAjouter: ISiteData
    {
        public string Url { get; set; }
        public string Titre { get; set; }
    }
    public class SiteAEditer: AvecIdUint, ISiteDataAnnulable
    {
        public string Url { get; set; }
        public string Titre { get; set; }
    }
    public class SiteVue : AKeyUidRno
    {
        public override string Uid { get; set; }
        public override int Rno { get; set; }
        public string Url { get; set; }
        public string Titre { get; set; }

        public bool? Ouvert { get; set; }
    }
}