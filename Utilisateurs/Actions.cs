using Microsoft.AspNetCore.Authorization.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KalosfideAPI.Utilisateurs
{
    public static class Actions
    {

        public static class Requirements
        {
            public static OperationAuthorizationRequirement Create = new OperationAuthorizationRequirement { Name = Noms.CreateOperationName };
            public static OperationAuthorizationRequirement Read =
              new OperationAuthorizationRequirement { Name = Noms.ReadOperationName };
            public static OperationAuthorizationRequirement Update =
              new OperationAuthorizationRequirement { Name = Noms.UpdateOperationName };
            public static OperationAuthorizationRequirement Delete =
              new OperationAuthorizationRequirement { Name = Noms.DeleteOperationName };
            public static OperationAuthorizationRequirement Approve =
              new OperationAuthorizationRequirement { Name = Noms.ApproveOperationName };
            public static OperationAuthorizationRequirement Reject =
              new OperationAuthorizationRequirement { Name = Noms.RejectOperationName };
        }
        public static class Noms
        {
            public const string CreateOperationName = "Ajoute";
            public const string ReadOperationName = "Lit";
            public const string UpdateOperationName = "Edite";
            public const string DeleteOperationName = "Supprime";
            public const string ApproveOperationName = "Approuve";
            public const string RejectOperationName = "RejectOperationName";
        }
    }
}
