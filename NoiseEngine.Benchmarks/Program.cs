using BenchmarkDotNet.Running;
using System;

BenchmarkRunner.Run(typeof(Program).Assembly, null, Environment.GetCommandLineArgs());
