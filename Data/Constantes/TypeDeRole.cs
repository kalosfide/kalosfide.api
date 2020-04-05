namespace KalosfideAPI.Data.Constantes
{
    public static class TypeDeRole
    {
        public static class Administrateur
        {
            public const string Code = "A";
            public const string Nom = "Administrateur";
        }
        public static class Fournisseur
        {
            public const string Code = "F";
            public const string Nom = "Fournisseur";
        }
        public static class Client
        {
            public const string Code = "C";
            public static string Nom(string nomFournisseur) => "Client de " + nomFournisseur;
        }
    }
}
