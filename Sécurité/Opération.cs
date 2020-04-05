using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Sécurité
{
    public class Opération
    {
        public string Nom { get; set; }
        public Restriction Restriction { get; set; } = Restriction.Aucune;
    }

    public enum Restriction
    {
        Aucune,
        PropriétaireDonnée,
        MembreSite,
        FournisseurSite, 
        Tous
    }

    public abstract class Contrainte
    {
        public abstract bool Autorise(CarteUtilisateur carteUtilisateur);
    }

    public abstract class ContraintePropriétaire
    {
        public abstract bool Autorise(CarteUtilisateur carteUtilisateur);
    }
}
