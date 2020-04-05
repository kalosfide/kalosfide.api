using KalosfideAPI.Data.Keys;

namespace KalosfideAPI.Fournisseurs
{
    public class FournisseurVue : AKeyUidRno
    {
        public override string Uid { get; set; }
        public override int Rno { get; set; }
        public string Nom { get; set; }
        public string Adresse { get; set; }
    }
}