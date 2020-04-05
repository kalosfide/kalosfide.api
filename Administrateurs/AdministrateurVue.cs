using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KalosfideAPI.Data.Keys;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace KalosfideAPI.Administrateurs
{
    public class AdministrateurVue: AKeyUidRno
    {
        public override string Uid { get; set; }
        public override int Rno { get; set; }
    }
}