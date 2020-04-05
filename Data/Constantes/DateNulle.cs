using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data.Constantes
{
    public static class DateNulle
    {
        public static DateTime Date = new DateTime(1970, 1, 1);
        
        public static bool Egale(DateTime date)
        {
            return DateTime.Compare(DateNulle.Date, date) == 0;
        }
    }
}
