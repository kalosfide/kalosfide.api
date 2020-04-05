using KalosfideAPI.Data;

namespace KalosfideAPI.Enregistrement
{
    public class EnregistrementClientVue : VueBase, IClient
    {
        public string Nom { get; set; }
        public string Adresse { get; set; }

        public string SiteUid { get; set; }
        public int SiteRno { get; set; }
    }
}
