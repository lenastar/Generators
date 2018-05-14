using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Generators;

namespace Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
        }
    }

    class Benchmark
    {
        protected readonly Generators.Expressions.ValueType<Address> evt;
        protected readonly Generators.ReflectionAPI.ValueType<Address> rvt;

        public Benchmark()
        {
            evt = new Generators.Expressions.ValueType<Address>();
            rvt = new Generators.ReflectionAPI.ValueType<Address>();
        }
    }

    class BenchmarkEquals : Benchmark
    {

        [Benchmark]
        public bool ReflectionEquals() => rvt.Equals(new Address("Stark", "Ova"));

        [Benchmark]
        public bool ExpressionEquals() => evt.Equals(new Address("Stark", "Ova"));
    }

    class BenchmarkToString : Benchmark
    {
        [Benchmark]
        public string ReflectionToString() => rvt.ToString();

        [Benchmark]
        public string ExpressionToString() => evt.ToString();
    }

    class BenchmarkGetHashCode : Benchmark
    {
        [Benchmark]
        public int ReflectionGetHashCode() => rvt.GetHashCode();

        [Benchmark]
        public int ExpressionGetHashCode() => evt.GetHashCode();
    }
}
