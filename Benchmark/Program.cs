using System;
using System.Threading;
using NUnit.Framework;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Generators;

namespace Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<Benchmark>();
            Thread.Sleep(600000);

        }
    }

    public class Benchmark
    {
        private readonly GenerationExpressions generationExpressions;
        private readonly GenerationEmit generationEmit;
        private readonly GenerationReflection generationReflection;

        public  Benchmark()
        {
            generationEmit = new GenerationEmit();
            generationExpressions = new GenerationExpressions();
            generationReflection = new GenerationReflection();
        }

        [Benchmark]
        public object ReflectionGetObject() => generationReflection.GetObject<Human>(typeof(Human));

        [Benchmark]
        public object ExpressionGetObject() => generationExpressions.GetObject<Human>(typeof(Human));
        [Benchmark]
        public object ReflectionEmitGetObject() => generationEmit.GetObject<Human>(typeof(Generators.Human));
    }

   




}
