using KalosfideAPI.Data;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Sites
{
    public interface ICréeSiteVue : IRoleData
    {

        public string Url { get; set; }
        public string Titre { get; set; }
    }

    public class CréeSiteVue: ICréeSiteVue
    {
        public string Nom { get; set; }
        public string Adresse { get; set; }
        public string Etat { get; set; }

        public string Url { get; set; }
        public string Titre { get; set; }

        /// <summary>
        /// Ville de signature des documents
        /// </summary>
        public string Ville { get; set; }

        public static void VérifieTrim(ICréeSiteVue vue, ModelStateDictionary modelState)
        {
            // Vérifie que Nom et Adresse sont présents et non vides
            Role.VérifieTrim(vue, modelState);
            if (vue.Url == null)
            {
                Erreurs.ErreurDeModel.AjouteAModelState(modelState, "url", "Absent");
            }
            else
            {
                vue.Url = vue.Url.Trim();
                if (vue.Url.Length == 0)
                {
                    Erreurs.ErreurDeModel.AjouteAModelState(modelState, "url", "Vide");
                }
            }
            if (vue.Titre == null)
            {
                Erreurs.ErreurDeModel.AjouteAModelState(modelState, "titre", "Absent");
            }
            else
            {
                vue.Titre = vue.Titre.Trim();
                if (vue.Titre.Length == 0)
                {
                    Erreurs.ErreurDeModel.AjouteAModelState(modelState, "titre", "Vide");
                }
            }
            if (vue.Ville == null)
            {
                Erreurs.ErreurDeModel.AjouteAModelState(modelState, "ville", "Absent");
            }
            else
            {
                vue.Ville = vue.Ville.Trim();
                if (vue.Ville.Length == 0)
                {
                    Erreurs.ErreurDeModel.AjouteAModelState(modelState, "ville", "Vide");
                }
            }
        }
    }
}
