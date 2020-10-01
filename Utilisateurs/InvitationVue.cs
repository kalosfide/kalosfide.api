using KalosfideAPI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Utilisateurs
{
    /// <summary>
    /// Invitation sans la key du Site
    /// Pour lecture de la liste
    /// </summary>
    public class InvitationVue
    {

        /// <summary>
        /// Email de l'utilisateur invité
        /// </summary>
        public string Email { get; set; }
        public DateTime Date { get; set; }

        // data
        public string UidClient { get; set; }
        public int? RnoClient { get; set; }

        public InvitationVue(Invitation invitation)
        {
            Email = invitation.Email;
            Date = invitation.Date;
            if (invitation.UidClient != null)
            {
                UidClient = invitation.UidClient;
                RnoClient = invitation.RnoClient;
            }
        }
    }

    public class InvitationsStock
    {
        public List<InvitationVue> Invitations { get; set; }
        public DateTime Date { get; set; }
    }
}
