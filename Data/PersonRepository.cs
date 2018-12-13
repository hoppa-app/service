using System;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

using hoppa.Service.Core;
using hoppa.Service.Model;
using hoppa.Service.Interfaces;

using MongoDB.Driver.Linq;

namespace hoppa.Service.Data
{
    public class PersonRepository : IPersonRepository
    {
        private readonly PersonContext _context = null;

        public PersonRepository(IOptions<Configuration> settings)
        {
            _context = new PersonContext(settings);
        }

        public async Task<IEnumerable<Person>> GetAllPeople()
        {
            try
            {
                return await _context.People.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        // query after Id or InternalId (BSonId value)
        //
        public async Task<Person> GetPerson(string Guid)
        {
            try
            {
                ObjectId internalId = GetInternalId(Guid);
                return await _context.People
                                .Find(Person => Person.Guid == Guid || Person.InternalId == internalId)
                                .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        // query after body text, updated time, and header image size
        //
        public async Task<IEnumerable<Person>> GetPerson(string Guid, string DisplayName)
        {
            try
            {
                var query = _context.People.Find(Person => Person.DisplayName.Contains(DisplayName));

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        // Try to convert the Id to a BSonId value
        private ObjectId GetInternalId(string Guid)
        {
            if (!ObjectId.TryParse(Guid, out ObjectId internalId))
                internalId = ObjectId.Empty;

            return internalId;
        }

        public async Task AddPerson(Person item)
        {
            try
            {
                await _context.People.InsertOneAsync(item);
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        public async Task<bool> RemovePerson(string Guid)
        {
            try
            {
                DeleteResult actionResult = await _context.People.DeleteOneAsync(
                     Builders<Person>.Filter.Eq("Guid", Guid));

                return actionResult.IsAcknowledged 
                    && actionResult.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        public async Task<bool> UpdatePerson(string Guid, string DisplayName)
        {
            var filter = Builders<Person>.Filter.Eq(s => s.Guid, Guid);
            var update = Builders<Person>.Update
                            .Set(s => s.DisplayName, DisplayName)
                            .CurrentDate(s => s.UpdatedOn);

            try
            {
                UpdateResult actionResult = await _context.People.UpdateOneAsync(filter, update);

                return actionResult.IsAcknowledged
                    && actionResult.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        public async Task<bool> UpdatePerson(string Guid, Person person)
        {
            try
            {
                ReplaceOneResult actionResult = await _context.People
                .ReplaceOneAsync(n => n.Guid.Equals(Guid)
                                , person
                                , new UpdateOptions { IsUpsert = true });
                
                return actionResult.IsAcknowledged
                    && actionResult.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        // Demo function - full document update
        public async Task<bool> UpdatePersonDocument(string Guid, string DisplayName)
        {
            var item = await GetPerson(Guid) ?? new Person();
            item.DisplayName = DisplayName;

            return await UpdatePerson(Guid, item);
        }

        public async Task<bool> RemoveAllPeople()
        {
            try
            {
                DeleteResult actionResult = await _context.People.DeleteManyAsync(new BsonDocument());

                return actionResult.IsAcknowledged
                    && actionResult.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        // it creates a sample compound index (first using UserId, and then Body)
        // 
        // MongoDb automatically detects if the index already exists - in this case it just returns the index details
        public async Task<string> CreateIndex()
        {
            try
            {
                IndexKeysDefinition <Person> keys = Builders<Person>
                                                    .IndexKeys
                                                    .Ascending(item => item.Guid)
                                                    .Ascending(item => item.DisplayName);

                return await _context.People
                                .Indexes.CreateOneAsync(new CreateIndexModel<Person>(keys));
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }
    }
}
