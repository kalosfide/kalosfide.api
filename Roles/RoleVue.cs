using KalosfideAPI.Data.Keys;

namespace KalosfideAPI.Roles
{
    public class RoleVue : AKeyUidRno
    {
        public override string Uid { get; set; }
        public override int Rno { get; set; }

        // données
        public string SiteUid { get; set; }
        public int SiteRno { get; set; }

        // calculés
        public string Etat { get; set; }
        public string Url { get; set; }
    }
}
