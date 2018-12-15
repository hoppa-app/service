namespace hoppa.Service.Core
{
    public class Configuration
    {
        public Service Service { get; set; } = new Service();

        public static Configuration Current;
    }

    public class Service
    {
        public MongoDB MongoDB { get; set; } = new MongoDB();
        public Intergrations Intergrations { get; set; } = new Intergrations ();
    }

    public class MongoDB
    {
        public string Url { get; set; }
        public string Database { get; set; }
        public string Collection { get; set; }
    }

    public class Intergrations
    {
        public OAuth2 Splitwise { get; set; } = new OAuth2();

        public OAuth2 bunq { get; set; } = new OAuth2();
    }

    public class OAuth2
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        public string RedirectUri { get; set; }
    }
}