using System;
using System.Linq;

namespace KalosfideAPI.Data.Constantes
{
    public static class TypeEtatRole
    {
        /// <summary>
        /// état d'un client qui a créé son compte et n'a pas encore été validé par le fournisseur
        /// ou état d'un fournisseur qui n'a pas encore été validé par l'administrateur.
        /// <remarks>
        /// Droits client: lire catalogue, commander, documents
        /// droits fournisseur: catalogue, clients, commander pour clients, livraison, documents
        /// </remarks>
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
        /// état d'un client qui a créé son compte, qui a des données et qui va quitter le site
        /// ou état d'un fournisseur dont le site va fermer
        /// pendant un certain temps il peut télécharger ses données
        /// </summary>
        public const string Invité = "V";

        /// <summary>
        /// état d'un client qui a des données et qui a quitté le site
        /// ou état d'un fournisseur dont le site a fermé
        /// ses données sont conservées mais ont été anonymisées
        /// </summary>
        public const string Exclu = "X";

        public static bool EstValide(string etat)
        {
            return (new string[]
            {
                Nouveau,
                Actif,
                Inactif,
                Exclu,
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
