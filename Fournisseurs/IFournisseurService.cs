using KalosfideAPI.Data;
using KalosfideAPI.Enregistrement;
using KalosfideAPI.Partages.KeyParams;

namespace KalosfideAPI.Fournisseurs
{
    public interface IFournisseurService : IKeyUidRnoService<Fournisseur, FournisseurVue>
    {
        Fournisseur CréeFournisseur(Role role, EnregistrementFournisseurVue fournisseurVue);
    }
}
