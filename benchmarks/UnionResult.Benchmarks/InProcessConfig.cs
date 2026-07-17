using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.Emit;

namespace UnionResult.Benchmarks;

/// <summary>
/// Runs benchmarks in-process instead of spawning a separate generated project.
/// BenchmarkDotNet 0.15.x does not yet recognize the net11.0 preview runtime moniker,
/// which makes its out-of-process SDK validation throw; running in-process skips
/// that validation entirely while still using MemoryDiagnoser for allocation data.
/// </summary>
public sealed class InProcessConfig : ManualConfig
{
    public InProcessConfig()
    {
        AddJob(Job.Default.WithToolchain(InProcessEmitToolchain.Instance));
        AddDiagnoser(MemoryDiagnoser.Default);
    }
}
