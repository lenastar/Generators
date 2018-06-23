using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generators
{
    interface IGeneration
    {
        Type GenerateClass(string name, Dictionary<string, Type> properties);
        T GetObject<T>(string nameOfClass);
    }
}
