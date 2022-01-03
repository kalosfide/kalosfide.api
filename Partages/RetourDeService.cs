using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Partages
{
    public enum TypeRetourDeService
    {
        Ok,
        IdentityError,
        ConcurrencyError,
        NotFound,
        UpdateError,
        ModelError,
        Indéterminé
    }

    public class RetourDeService
    {
        public TypeRetourDeService Type { get; set; }
        public object Objet { get; set; }
        public string Message { get; set; }

        public bool Ok { get { return this.Type == TypeRetourDeService.Ok; } }
        public bool ConcurrencyError { get { return this.Type == TypeRetourDeService.ConcurrencyError; } }
        public bool IdentityError { get { return this.Type == TypeRetourDeService.IdentityError; } }
        public bool UpdateError { get { return this.Type == TypeRetourDeService.UpdateError; } }
        public bool ModelError { get { return this.Type == TypeRetourDeService.ModelError; } }

        public RetourDeService(TypeRetourDeService type)
        {
            Type = type;
        }

        public RetourDeService(object Objet)
        {
            Type = TypeRetourDeService.Ok;
            this.Objet = Objet;
        }

        public RetourDeService(IdentityResult result)
        {
            Type = TypeRetourDeService.IdentityError;
            Objet = result.Errors;
        }

        public RetourDeService(RetourDeService retour)
        {
            Type = retour.Type;
            Objet = retour.Objet;
            Message = retour.Message;
        }

    }

    public class RetourDeService<T> : RetourDeService where T: class
    {
        public T Entité
        {
            get => Objet as T;
            set => base.Objet = value;
        }
        public RetourDeService(TypeRetourDeService type) : base(type) { }

        public RetourDeService(T Entité) : base(Entité) { }

        public RetourDeService(IdentityResult result): base(result) { }

        public RetourDeService(RetourDeService retour): base(retour) { }
    }
}
