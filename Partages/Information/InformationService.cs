using KalosfideAPI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Partages
{
    public class InformationService : BaseService, IInformationService
    {
        public InformationService(ApplicationContext context) : base(context)
        {
        }
    }
}
