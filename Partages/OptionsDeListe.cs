using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace KalosfideAPI.Partages
{
    public class FiltreDeListe
    {
        public string Nom { get; set; }
        public string Valeur { get; set; }
        public string Comparaison { get; set; }
    }
    public delegate IQueryable<T> AppliqueFiltreDeListe<T>(IQueryable<T> queryable, FiltreDeListe filtre);
    public class FiltreurDeListe<T>
    {
        public string Nom { get; set; }
        public AppliqueFiltreDeListe<T> AppliqueFiltre { get; set; }
    }

    public class TriDeListe
    {
        public string Nom { get; set; }
        public bool Desc { get; set; }
    }
    public delegate IQueryable<T> AppliqueTriDeListe<T>(IQueryable<T> queryable, TriDeListe tri);
    public class TrieurDeListe<T>
    {
        public string Nom { get; set; }
        public AppliqueTriDeListe<T> AppliqueTri { get; set; }
    }

    class _OptionsDeListe
    {
        public FiltreDeListe[] Filtres { get; private set; }
        public TriDeListe[] Tris { get; private set; }
    }
    public class OptionsDeListe
    {
        private _OptionsDeListe _options { get; set; }

        private string _id;
        public string Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
                _options = JsonConvert.DeserializeObject<_OptionsDeListe>(value);
            }
        }
        public FiltreDeListe[] Filtres
        {
            get
            {
                return _options.Filtres;
            }
        }
        public TriDeListe[] Tris
        {
            get
            {
                return _options.Tris;
            }
        }
    }
}
