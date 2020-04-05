using KalosfideAPI.Data;
using KalosfideAPI.Enregistrement;
using KalosfideAPI.Partages.KeyParams;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace KalosfideAPI.Fournisseurs
{
    class GèreArchive : Partages.KeyParams.GéreArchive<Fournisseur, FournisseurVue, ArchiveFournisseur>
    {
        public GèreArchive(DbSet<Fournisseur> dbSet, DbSet<ArchiveFournisseur> dbSetArchive) : base(dbSet, dbSetArchive)
        { }

        protected override ArchiveFournisseur CréeArchive()
        {
            return new ArchiveFournisseur();
        }

        protected override void CopieDonnéeDansArchive(Fournisseur donnée, ArchiveFournisseur archive)
        {
            archive.Nom = donnée.Nom;
            archive.Adresse = donnée.Adresse;
        }

        protected override ArchiveFournisseur CréeArchiveDesDifférences(Fournisseur donnée, FournisseurVue vue)
        {
            bool modifié = false;
            ArchiveFournisseur archive = new ArchiveFournisseur
            {
                Date = DateTime.Now
            };
            if (vue.Nom != null && donnée.Nom != vue.Nom)
            {
                donnée.Nom = vue.Nom;
                archive.Nom = vue.Nom;
                modifié = true;
            }
            if (vue.Adresse != null && donnée.Adresse != vue.Adresse)
            {
                donnée.Adresse = vue.Adresse;
                archive.Adresse = vue.Adresse;
                modifié = true;
            }
            return modifié ? archive : null;
        }
    }

    public class FournisseurService : KeyUidRnoService<Fournisseur, FournisseurVue>, IFournisseurService
    {
        public FournisseurService(ApplicationContext context) : base(context)
        {
            _dbSet = _context.Fournisseur;
            _géreArchive = new GèreArchive(_dbSet, _context.ArchiveFournisseur);
        }

        public Fournisseur CréeFournisseur(Role role, EnregistrementFournisseurVue fournisseurVue)
        {
            Fournisseur fournisseur = new Fournisseur
            {
                Uid = role.Uid,
                Rno = role.Rno,
                Nom = fournisseurVue.Nom,
                Adresse = fournisseurVue.Adresse
            };
            return fournisseur;
        }

        public override void CopieVueDansDonnée(Fournisseur donnée, FournisseurVue vue)
        {
            if (vue.Nom != null)
            {
                donnée.Nom = vue.Nom;
            }
            if (vue.Adresse != null)
            {
                donnée.Adresse = vue.Adresse;
            }
        }

        public override void CopieVuePartielleDansDonnée(Fournisseur donnée, FournisseurVue vue, Fournisseur donnéePourComplèter)
        {
            donnée.Nom = vue.Nom ?? donnéePourComplèter.Nom;
            donnée.Adresse = vue.Adresse ?? donnéePourComplèter.Adresse;
        }

        public override Fournisseur CréeDonnée()
        {
            return new Fournisseur();
        }

        public override FournisseurVue CréeVue(Fournisseur donnée)
        {
            FournisseurVue vue = new FournisseurVue
            {
                Nom = donnée.Nom,
                Adresse = donnée.Adresse,
            };
            vue.CopieKey(donnée.KeyParam);
            return vue;
        }
    }
}
