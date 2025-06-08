using System;

namespace MacAgent.Components;

public class CpuCore
{
    public string Name { get; set; } = string.Empty;

    public UInt64 PercentProcessorTime { get; set; }
}