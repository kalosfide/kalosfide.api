using KalosfideAPI.Partages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Peuple
{
    public interface IPeupleService
    {
        Task<bool> EstPeuplé();
        Task<RetourDeService> Peuple();
    }
}
