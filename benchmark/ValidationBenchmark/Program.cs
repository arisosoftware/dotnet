// See https://aka.ms/new-console-template for more information


// version 1, just default benchmark     output
// using BenchmarkDotNet.Running;
// BenchmarkRunner.Run<ValidationBenchmarks>();

// version 2, add custom config to the benchmark output
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;

var config = ManualConfig.Create(DefaultConfig.Instance)
    .WithSummaryStyle(SummaryStyle.Default.WithMaxParameterColumnWidth(40))
    .WithOption(ConfigOptions.JoinSummary, true)
    .AddExporter(JsonExporter.Full) // <--- Export full JSON
    .AddColumnProvider(DefaultColumnProviders.Instance);

BenchmarkRunner.Run<ValidationBenchmarks>(config);

// readme :
