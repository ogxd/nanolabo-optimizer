﻿using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using System;
using System.Linq;

namespace Nanomesh.Benchmarks
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            ManualConfig conf = new ManualConfig();
            conf.AddExporter(DefaultConfig.Instance.GetExporters().ToArray());
            conf.AddLogger(DefaultConfig.Instance.GetLoggers().ToArray());
            conf.AddColumnProvider(DefaultConfig.Instance.GetColumnProviders().ToArray());
            conf.AddDiagnoser(MemoryDiagnoser.Default);

            var switcher = new BenchmarkSwitcher(new[] {
                typeof(Benchmark),
            });

            switcher.Run(args, config: conf);

            Console.ReadKey();
        }
    }
}
