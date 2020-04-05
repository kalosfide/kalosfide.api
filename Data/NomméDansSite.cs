using KalosfideAPI.Data.Keys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data
{
    public interface INomméDansSite
    {
        string Uid { get; set; }
        int Rno { get; set; }
        string Nom { get; set; }
    }
}
