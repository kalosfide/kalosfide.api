using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data.Keys
{
    public class AvecIdUintDate : AvecIdUint, IAvecDate
    {
        public DateTime Date { get; set; }
    }

    public class Ajouté<T> : IAvecDate where T : AvecIdUint
    {
        public T Donnée { get; set; }
        public DateTime Date { get; set; }

        public Ajouté(T donnée, DateTime date)
        {
            Donnée = donnée;
            Date = date;
        }

        public AvecIdUintDate AvecIdUintDate {
            get
            {
                return new AvecIdUintDate
                {
                    Id = Donnée.Id,
                    Date = this.Date
                };
            }
}
    }
}
