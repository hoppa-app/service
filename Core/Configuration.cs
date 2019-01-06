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

        public SendGrid SendGrid { get; set; } = new SendGrid();
        public Intergrations Intergrations { get; set; } = new Intergrations ();
    }

    public class MongoDB
    {
        public string Url { get; set; }
        public string Database { get; set; }
        public string Collection { get; set; }
    }

    public class SendGrid
    {
        public string ApiKey { get; set; }
    }

    public class Intergrations
    {
        public OAuth2 Splitwise { get; set; } = new OAuth2();

        public OAuth2 bunq { get; set; } = new OAuth2();

        public RabobankOAuth2 Rabobank { get; set; } = new RabobankOAuth2();

        public INGOAuth2 ING { get; set; } = new INGOAuth2();
    }

    public class OAuth2
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        public string RedirectUri { get; set; }
    }

    public class RabobankOAuth2 : OAuth2
    {
        public string ApiUri { get; set; }

        public Certificates Certificates { get; set; } = new Certificates();
    }

    public class INGOAuth2 : OAuth2
    {
        public string ApiUri { get; set; }

        public Certificates Certificates { get; set; } = new Certificates();
    }

    public class Certificates
    {
        public Certificate Signing { get; set; } = new Certificate();
        public Certificate TLS { get; set; } = new Certificate();
    }

    public class Certificate
    {
        public string Thumbprint { get; set; }
        public string SerialNr { get; set; }
    }
}