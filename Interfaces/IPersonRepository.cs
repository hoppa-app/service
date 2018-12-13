using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using hoppa.Service.Model;

namespace hoppa.Service.Interfaces
{
    public interface IPersonRepository
    {
        Task<IEnumerable<Person>> GetAllPeople();

        Task<Person> GetPerson(string Guid);

        // query after multiple parameters
        Task<IEnumerable<Person>> GetPerson(string Guid, string DisplayName);

        // add new Person document
        Task AddPerson(Person item);

        // remove a single document / Person
        Task<bool> RemovePerson(string Guid);

        // update just a single document / Person
        Task<bool> UpdatePerson(string Guid, string DisplayName);

        Task<bool> UpdatePerson(string Guid, Person person);

        // demo interface - full document update
        Task<bool> UpdatePersonDocument(string Guid, string DisplayName);

        // should be used with high cautious, only in relation with demo setup
        Task<bool> RemoveAllPeople();

        // creates a sample index
        Task<string> CreateIndex();
    }
}
