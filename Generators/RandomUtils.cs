using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generators
{
    internal static class RandomUtils
    {
        public static readonly Dictionary<Type, Func<object>> RandomUtilDictionary = new Dictionary<Type, Func<object>>()
        {
            { typeof(int), () => GetRandomInt() },
            { typeof(double), () => GetRandomDouble() },
            { typeof(string), () => GetRandomString() },
            { typeof(bool), () => GetRandomBool() },
            { typeof(char), () => GetRandomChar() }
        };

        private static readonly char[] Chars = "$%#@!*abcdefghijklmnopqrstuvwxyz1234567890?;:ABCDEFGHIJKLMNOPQRSTUVWXYZ^&".ToCharArray();

        private static int GetRandomInt(int start = -100, int end = 100)
        {
            return new Random().Next(start, end);
        }

        private static double GetRandomDouble()
        {
            return new Random().NextDouble();
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
            return Chars[new Random().Next(Chars.Length)];
        }

        private static bool GetRandomBool()
        {
            var bools = new[] { true, false };
            return bools[new Random().Next(0, 1)];
        }

        public static List<object> GetRandomList<T1>(int length = 10)
        {
            var enumerable = new List<object>();
            for (var i = 0; i < length; i++)
            {
                enumerable.Add(RandomUtilDictionary[typeof(T1)]());
            }

            return enumerable;
        }

    }
}
