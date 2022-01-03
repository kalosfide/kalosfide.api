using KalosfideAPI.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Clients
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
        public uint? ClientId { get; set; }

        public InvitationVue(Invitation invitation)
        {
            Email = invitation.Email;
            Date = invitation.Date;
            if (invitation.ClientId != null)
            {
                ClientId = invitation.ClientId;
            }
        }
    }

    public class InvitationsStock
    {
        public List<InvitationVue> Invitations { get; set; }
        public DateTime Date { get; set; }
    }

    /// <summary>
    /// Objet contenant le Fournisseur incluant son Site et éventuellement le Client à prendre en charge d'une Invitation.
    /// </summary>
    [JsonObject(MemberSerialization=MemberSerialization.OptIn)]
    public class InvitationContexte
    {
        [JsonProperty]
        public Fournisseur Fournisseur { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Client Client { get; set; }
    }
}
