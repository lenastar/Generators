
using Generators.Domains;

namespace Generators
{
    public class Address:IDomain
    {
        public Address(string street, string building)
        {
            Street = street;
            Building = building;
        }

        public string Street { get; }
        public string Building { get; }
    }
}