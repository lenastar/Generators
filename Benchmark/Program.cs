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
        private readonly Generators.Expressions.ValueType<Address> evt;
        private readonly Generators.ReflectionAPI.ValueType<Address> rvt;

        public Benchmark()
        {
            evt = new Generators.Expressions.ValueType<Address>();
            rvt = new Generators.ReflectionAPI.ValueType<Address>();
        }

     //   [Benchmark]
    //    public bool ReflectionEquals() => rvt.Equals();


    }
}
