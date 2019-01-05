using Microsoft.Extensions.Options;
using MongoDB.Driver;

using hoppa.Service.Core;
using hoppa.Service.Model;

namespace hoppa.Service.Data
{
    public class PersonContext
    {
        private readonly IMongoDatabase _database = null;

        public PersonContext()
        {
            var client = new MongoClient(Configuration.Current.Service.MongoDB.Url);
            if (client != null)
                _database = client.GetDatabase(Configuration.Current.Service.MongoDB.Database);
        }

        public IMongoCollection<Person> People
        {
            get
            {
                return _database.GetCollection<Person>(Configuration.Current.Service.MongoDB.Collection);
            }
        }
    }
}
