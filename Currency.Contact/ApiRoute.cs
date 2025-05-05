namespace Currencey.Contact;

public static class ApiRoute
{
    const string  Api="api/";
    
    public const string login = Api + "login";
    public const string logout = Api + "logout";
    public const string currentUser = Api + "me";
    
    
    public static class Currency
    {
        const string Base = Api + "currency";
        public const string GetAll = Base;
        public const string GetById = Base + "/{id}";
        public const string Update = Base;
    }
}
