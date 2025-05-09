namespace Currencey.Contact;

public static class ApiRoute
{
    const string Api = "api/";

    public const string login = Api + "login";
    public const string logout = Api + "logout";
    public const string changePassword = Api + "changePassword";
    public const string currentUser = Api + "me";
    public const string refreshToken = Api + "refresh";


    public static class Currency
    {
        const string Base = Api + "currency";
        public const string GetAll = Base;
        public const string GetById = Base + "/{id}";
        public const string Update = Base;
    }
}
