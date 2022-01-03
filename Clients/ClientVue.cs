using System;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using KalosfideAPI.Utiles;

namespace KalosfideAPI.Clients
{
    /// <summary>
    /// Contient les données de Client et l'Etat annulable sans l'Id du Site.
    /// </summary>
    public class ClientAEditer : AvecIdUint, IClientDataAnnullable
    {
        public string Nom { get; set; }
        public string Adresse { get; set; }
        public string Ville { get; set; }
        public EtatRole? Etat { get; set; }
    }

    /// <summary>
    /// Contient l'Id du Site et les données de Client sans Etat.
    /// </summary>
    public class ClientAAjouter : IClientData
    {
        /// <summary>
        /// Id du site
        /// </summary>
        public uint SiteId { get; set; }

        public string Nom { get; set; }
        public string Adresse { get; set; }
        public string Ville { get; set; }
    }

    public class ClientData : IClientData
    {
        public string Nom { get; set; }
        public string Adresse { get; set; }

        public string Ville { get; set; }

    }
    public class ClientEtatVue : AvecIdUint, IClientData
    {
        public string Nom { get; set; }
        public string Adresse { get; set; }

        public string Ville { get; set; }

        public EtatRole Etat { get; set; }

        public DateTime Date0 { get; set; }

        public DateTime DateEtat { get; set; }

        public string Email { get; set; }

        public bool AvecDocuments { get; set; }

    }
}