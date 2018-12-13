using Microsoft.Extensions.Options;
using MongoDB.Driver;

using hoppa.Service.Core;
using hoppa.Service.Model;

namespace hoppa.Service.Data
{
    public class PersonContext
    {
        private readonly IMongoDatabase _database = null;

        public PersonContext(IOptions<Configuration> settings)
        {
            var client = new MongoClient(settings.Value.Service.MongoDB.Url);
            if (client != null)
                _database = client.GetDatabase(settings.Value.Service.MongoDB.Database);
        }

        public IMongoCollection<Person> People
        {
            get
            {
                return _database.GetCollection<Person>("people");
            }
        }
    }
}
