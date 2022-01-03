using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data.Keys
{
    public interface IAvecSiteId: IAvecDate
    {
        uint SiteId { get; set; }
    }
}
