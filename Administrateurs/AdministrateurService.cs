using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Partages.KeyParams;
using System.Threading.Tasks;

namespace KalosfideAPI.Administrateurs
{
    public class AdministrateurService : KeyUidRnoService<Administrateur, AdministrateurVue>, IAdministrateurService
    {
        public AdministrateurService(ApplicationContext context) : base(context)
        {
            _dbSet = context.Administrateur;
        }

        public override void CopieVueDansDonnée(Administrateur donnée, AdministrateurVue vue)
        {
        }

        public override void CopieVuePartielleDansDonnée(Administrateur donnée, AdministrateurVue vue, Administrateur donnéePourComplèter)
        {
        }

        public override Administrateur CréeDonnée()
        {
            return new Administrateur();
        }

        public override AdministrateurVue CréeVue(Administrateur donnée)
        {
            AdministrateurVue vue = new AdministrateurVue();
            vue.CopieKey(donnée.KeyParam);
            return vue;
        }
    }
}
