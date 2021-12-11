using System;
using System.Linq;

namespace KalosfideAPI.Data.Constantes
{
    public static class TypeEtatRole
    {
        /// <summary>
        /// Etat d'un client qui vient de répondre à une invitation.
        /// Ce client ne peut pas commander tant que le fournisseur ne l'a pas activé.
        /// Etat d'un fournisseur qui vient d'envoyer sa demande de devenir fournisseur.
        /// Ce fournisseur ne peut rien faire tant qu'un administrateur ne l'a pas activé.
        /// </summary>
        public const string Nouveau = "N";

        /// <summary>
        /// état d'un client qui a créé son compte et a été validé par le fournisseur
        /// ou état d'un client qui a été créé par le fournisseur
        /// ou état d'un fournisseur qui a été validé par l'administrateur
        /// les commandes d'un tel client n'ont pas besoin d'être validées lors de la réception par le fournisseur
        /// droits client: lire catalogue, commander, documents
        /// droits fournisseur: catalogue, clients, commander pour clients, livraison, documents
        /// </summary>
        public const string Actif = "A";

        /// <summary>
        /// état d'un client qui a créé son compte, qui a des données et qui va quitter le site
        /// ou état d'un fournisseur dont le site va fermer
        /// pendant un certain temps il peut télécharger ses données
        /// </summary>
        public const string Inactif = "I";

        /// <summary>
        /// état d'un client qui a des données et qui a quitté le site
        /// ou état d'un fournisseur dont le site a fermé
        /// ses données sont conservées mais ont été anonymisées
        /// </summary>
        public const string Fermé = "F";

        public static bool EstValide(string etat)
        {
            return (new string[]
            {
                Nouveau,
                Actif,
                Inactif,
                Fermé,
            }).Contains(etat);
        }

        public static int JoursInactifAvantExclu()
        {
            return 60;
        }
        public static long TicksInactifAvantExclu()
        {
            return 24 * 60 * 60 * 1000 * JoursInactifAvantExclu();
        }
    }
}
