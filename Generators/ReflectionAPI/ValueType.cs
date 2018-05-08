using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Generators
{
    /// <summary>
    /// Базовый класс для всех Value типов.
    /// </summary>
    public class ValueType<T>
    {
        public override string ToString()
        {
            var sortedProperties = GetPropertiesValues(this)
                .OrderBy(x => x.Item1)
                .Select(tuple => $"{tuple.Item1}: {tuple.Item2}");

            return typeof(T).Name + '(' + string.Join("; ", sortedProperties) + ')';
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return GetPropertiesValues(this)
                .Aggregate(0, (x, y) => x.GetHashCode()*5 ^ y.GetHashCode()*3);
            } 
        }

        public override bool Equals(object obj)
        {
            var objProperties = GetPropertiesValues(obj);
            var thisProperties = GetPropertiesValues(this);

            return objProperties
                .Zip(thisProperties, (x, y) => x.Equals(y))
                .All(x => x);
        }

        public bool Equals(T value)
        {
            return value != null && value.Equals(this);
        }

        private List<Tuple<string, object>> GetPropertiesValues(object obj)
        {
            return obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(x => Tuple.Create(x.Name, x.GetValue(obj)))
                .ToList();
        }
    }
}