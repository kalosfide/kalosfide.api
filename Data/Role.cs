﻿using KalosfideAPI.Data.Constantes;
using KalosfideAPI.Data.Keys;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace KalosfideAPI.Data
{
    public enum EtatRole
    {
        Nouveau,
        Actif,
        Inactif,
        Fermé
    }

    public static class EtatsRolePermis
    {
        public static EtatRole[] Nouveau { get { return new EtatRole[] { EtatRole.Nouveau }; } }
        public static EtatRole[] Actif { get { return new EtatRole[] { EtatRole.Actif }; } }
        public static EtatRole[] PasInactif { get { return new EtatRole[] { EtatRole.Nouveau, EtatRole.Actif }; } }
        public static EtatRole[] PasFermé { get { return new EtatRole[] { EtatRole.Nouveau, EtatRole.Actif, EtatRole.Inactif }; } }
    }

    // Interfaces communs aux entités Client et Fournisseur

    public interface IRoleData
    {
        string Nom { get; set; }
        string Adresse { get; set; }
        string Ville { get; set; }
    }

    public interface IAvecEtat
    {
        EtatRole Etat { get; set; }
    }
    public interface IAvecIdUint
    {
        uint Id { get; set; }
    }
    public interface IAvecIdUintEtEtat: IAvecIdUint, IAvecEtat
    {
    }

    public interface IAvecEtatAnnulable
    {
        EtatRole? Etat { get; set; }
    }
    public interface IAvecIdUintEtDateEtEtatAnnulable: IAvecIdUint, IAvecDate, IAvecEtatAnnulable
    {
    }

    public interface IRoleDataAnnulable: IRoleData, IAvecEtatAnnulable { }

    public interface IArchiveRole: IAvecEtatAnnulable, IAvecDate
    { }

    public interface IRoleEtat: IAvecEtat
    {
        /// <summary>
        /// Date de création.
        /// </summary>
        public DateTime Date0 { get; set; }

        /// <summary>
        /// Date du dernier changement d'état.
        /// </summary>
        public DateTime DateEtat { get; set; }

    }

    public interface IRolePréférences
    { 
        string FormatNomFichierCommande { get; set; }
        string FormatNomFichierLivraison { get; set; }
        string FormatNomFichierFacture { get; set; }
    }

    public static class Role
    {
        /// <summary>
        /// Chaîne de caractère où {no} représente le numéro du document et {nom} le nom du client si l'utilisateur est le fournisseur
        /// ou du fournisseur si l'utilisateur est le client
        /// </summary>
        public const string FormatNomFichierCommandeParDéfaut = "{nom} commande {no}";

        /// <summary>
        /// Chaîne de caractère où {no} représente le numéro du document et {nom} le nom du client si l'utilisateur est le fournisseur
        /// ou du fournisseur si l'utilisateur est le client
        /// </summary>
        public const string FormatNomFichierLivraisonParDéfaut = "{nom} livraison {no}";

        /// <summary>
        /// Chaîne de caractère où {no} représente le numéro du document et {nom} le nom du client si l'utilisateur est le fournisseur
        /// ou du fournisseur si l'utilisateur est le client
        /// </summary>
        public const string FormatNomFichierFactureParDéfaut = "{nom} facture {no}";

        public static void CopieData(IRoleData de, IRoleData vers)
        {
            vers.Nom = de.Nom;
            vers.Adresse = de.Adresse;
            vers.Ville = de.Ville;
        }

        public static void CopieData(IRoleDataAnnulable de, IRoleDataAnnulable vers)
        {
            vers.Nom = de.Nom;
            vers.Adresse = de.Adresse;
            vers.Ville = de.Ville;
            vers.Etat = de.Etat;
        }
        public static void CopieDataSiPasNull(IRoleDataAnnulable de, IRoleData vers)
        {
            if (de.Nom != null) { vers.Nom = de.Nom; }
            if (de.Adresse != null) { vers.Adresse = de.Adresse; }
            if (de.Ville != null) { vers.Ville = de.Ville; }
        }
        public static void CopieDataSiPasNullOuComplète(IRoleDataAnnulable de, IRoleData vers, IRoleData pourCompléter)
        {
            vers.Nom = de.Nom ?? pourCompléter.Nom;
            vers.Adresse = de.Adresse ?? pourCompléter.Adresse;
            vers.Ville = de.Ville ?? pourCompléter.Ville;
        }

        /// <summary>
        /// Si un champ du nouvel objet à une valeur différente de celle du champ correspondant de l'ancien objet,
        /// met à jour l'ancien objet et place ce champ dans l'objet des différences.
        /// </summary>
        /// <param name="ancien"></param>
        /// <param name="nouveau"></param>
        /// <param name="différences"></param>
        /// <returns>true si des différences ont été enregistrées</returns>
        public static bool CopieDifférences(IRoleData ancien, IRoleDataAnnulable nouveau, IRoleDataAnnulable différences)
        {
            bool modifié = false;
            if (nouveau.Nom != null && ancien.Nom != nouveau.Nom)
            {
                différences.Nom = nouveau.Nom;
                ancien.Nom = nouveau.Nom;
                modifié = true;
            }
            if (nouveau.Adresse != null && ancien.Adresse != nouveau.Adresse)
            {
                différences.Adresse = nouveau.Adresse;
                ancien.Adresse = nouveau.Adresse;
                modifié = true;
            }
            if (nouveau.Ville != null && ancien.Ville != nouveau.Ville)
            {
                différences.Ville = nouveau.Ville;
                ancien.Ville = nouveau.Ville;
                modifié = true;
            }
            return modifié;
        }

        public static string[] AvérifierSansEspacesData
        {
            get
            {
                return new string[]
                {
                    nameof(IRoleData.Nom),
                    nameof(IRoleData.Adresse),
                    nameof(IRoleData.Ville)
                };
            }
        }

        public static string[] AvérifierSansEspacesDataAnnulable
        {
            get
            {
                return new string[]
                {
                    nameof(IRoleData.Nom),
                    nameof(IRoleData.Adresse),
                    nameof(IRoleData.Ville)
                };
            }
        }

        public static void CopiePréférences(IRolePréférences de, IRolePréférences vers)
        {
            vers.FormatNomFichierCommande = de.FormatNomFichierCommande;
            vers.FormatNomFichierLivraison = de.FormatNomFichierLivraison;
            vers.FormatNomFichierFacture = de.FormatNomFichierFacture;
        }

        private static void CopieArchivesEtat(IEnumerable<IArchiveRole> archives, IRoleEtat roleEtat)
        {
            IEnumerable<IArchiveRole> archivesDansLordre = archives.Where(a => a.Etat != null).OrderBy(a => a.Date);
            IArchiveRole création = archivesDansLordre.First();
            IArchiveRole actuel = archivesDansLordre.Last();
            roleEtat.Etat = actuel.Etat.Value;
            roleEtat.Date0 = création.Date;
            roleEtat.DateEtat = actuel.Date;
        }
        public static void CopieArchivesEtat(Client client, IRoleEtat roleEtat)
        {
            CopieArchivesEtat((IEnumerable<IArchiveRole>)client.Archives, roleEtat);
        }

    }

}