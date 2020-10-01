using KalosfideAPI.Data;
using Microsoft.AspNetCore.Mvc;
using System;

namespace KalosfideAPI.Utilisateurs
{
    public class InvitationVérifiée
    {
        public IActionResult Result { get; set; }
        public Site Site { get; set; }
        public Utilisateur Utilisateur { get; set; }
        public Client Client { get; set; }
        public Invitation Invitation { get; set; }

        public bool AMêmeClient()
        {
            return Client == null
                ? Invitation.UidClient == null
                : Invitation.UidClient == Client.Uid && Invitation.RnoClient == Client.Rno;
        }

        public bool InvitationPérimée()
        {
            return (DateTime.Now - Invitation.Date).TotalDays > 1;
        }
    }
}
