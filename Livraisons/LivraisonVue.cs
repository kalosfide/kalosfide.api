using KalosfideAPI.Commandes;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using System;
using System.Collections.Generic;

namespace KalosfideAPI.Livraisons
{
    public class LivraisonVue : AKeyUidRnoNo
    {
        public override string Uid { get; set; }
        public override int Rno { get; set; }
        public override long No { get; set; }

        public DateTime? DateLivraison { get; set; }

        public DateTime? Date { get; set; }

        public List<CommandeVue> Commandes { get; set; }

    }
}
