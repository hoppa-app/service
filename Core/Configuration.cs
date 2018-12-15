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
    }

    public class MongoDB
    {
        public string Url { get; set; }
        public string Database { get; set; }
        public string Collection { get; set; }
    }
}