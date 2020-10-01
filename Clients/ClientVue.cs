using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KalosfideAPI.Data;
using KalosfideAPI.Data.Keys;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace KalosfideAPI.Clients
{
    public class ClientVue : AKeyUidRno, IClient
    {
        public override string Uid { get; set; }
        public override int Rno { get; set; }
        public string Nom { get; set; }
        public string Adresse { get; set; }
    }
    public class ClientVueAjoute : AKeyUidRno, IClient
    {
        public override string Uid { get; set; }
        public override int Rno { get; set; }
        public string Nom { get; set; }
        public string Adresse { get; set; }
    }
    public class ClientEtatVue : AKeyUidRno
    {
        public override string Uid { get; set; }
        public override int Rno { get; set; }

        public string Nom { get; set; }
        public string Adresse { get; set; }

        public string Etat { get; set; }

        public DateTime DateEtat { get; set; }

        public string Compte { get; set; }

        public bool AvecCommandes { get; set; }

    }
}