
using Generators.Domains;

namespace Generators
{
    public class PersonName:IDomain
    {
        public PersonName(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }

        public string FirstName { get; }
        public string LastName { get; }
    }
}