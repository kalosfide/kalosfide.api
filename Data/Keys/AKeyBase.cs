using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Data.Keys
{
    public abstract class AKeyBase
    {
        public static string Séparateur = "-";

        [JsonIgnore]
        public abstract KeyParam KeyParam { get; }
        [JsonIgnore]
        public abstract KeyParam KeyParamParent { get; }
    }
}
