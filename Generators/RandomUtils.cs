using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generators
{
    internal static class RandomUtils
    {
        private static readonly Dictionary<Type, Func<object>> RandomUtilDictionary 
            = new Dictionary<Type, Func<object>>
        {
            { typeof(int), () => GetRandomInt() },
            { typeof(double), () => GetRandomDouble() },
            { typeof(string), () => GetRandomString() },
            { typeof(bool), () => GetRandomBool() },
            { typeof(char), () => GetRandomChar() }
        };

        private static readonly Random random = new Random();

        private static readonly char[] Chars = 
            "$%#@!*abcdefghijklmnopqrstuvwxyz1234567890?;:ABCDEFGHIJKLMNOPQRSTUVWXYZ^&"
                .ToCharArray();

        public static object GetRandomObject(Type type)
        {
            return RandomUtilDictionary.ContainsKey(type) 
                ? RandomUtilDictionary[type]()
                : null;
        }

        public static IEnumerable<T> GetRandomEnumerable<T>(int length = 10)
        {
            var enumerable = new List<T>();
            for (var i = 0; i < length; i++)
            {
                enumerable.Add((T)GetRandomObject(typeof(T)));
            }

            return enumerable;
        }

        private static int GetRandomInt(int start = -100, int end = 100)
        {
            return random.Next(start, end);
        }

        private static double GetRandomDouble()
        {
            return random.NextDouble();
        }

        private static string GetRandomString(int length = 10)
        {
            var result = new StringBuilder();
            for (var i = 1; i < length; i++)
            {
                result.Append(GetRandomChar());
            }

            return result.ToString();
        }

        private static char GetRandomChar()
        {
            return Chars[random.Next(Chars.Length)];
        }

        private static bool GetRandomBool()
        {
            return random.Next(2) == 1;
        }
    }
}
