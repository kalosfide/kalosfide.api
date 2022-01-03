using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KalosfideAPI.Roles
{
    public class RoleEtat: IRoleEtat
    {
        /// <summary>
        /// Une des valeurs de TypeEtatRole.
        /// </summary>
        public EtatRole Etat { get; set; }

        /// <summary>
        /// Date de création.
        /// </summary>
        public DateTime Date0 { get; set; }

        /// <summary>
        /// Date du dernier changement d'état.
        /// </summary>
        public DateTime DateEtat { get; set; }

        /// <summary>
        /// RoleEtat ne contenant que la date de l'état.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static RoleEtat DeDate(DateTime date)
        {
            return new RoleEtat
            {
                DateEtat = date
            };
        }
    }
}
