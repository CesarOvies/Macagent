using System;
using System.Collections.Generic;

// https://docs.microsoft.com/en-us/windows/win32/cimwin32prov/win32-processor

namespace MacAgent.Components;

public class CPU
{
    public string Caption { get; set; } = string.Empty;

    public UInt32 CurrentClockSpeed { get; set; }

    public string Description { get; set; } = string.Empty;

    public UInt32 L1InstructionCacheSize { get; set; }

    public UInt32 L1DataCacheSize { get; set; }

    public UInt32 L2CacheSize { get; set; }

    public UInt32 L3CacheSize { get; set; }

    public string Manufacturer { get; set; } = string.Empty;

    public UInt32 MaxClockSpeed { get; set; }

    public string Name { get; set; } = string.Empty;

    public UInt32 NumberOfCores { get; set; }

    public UInt32 NumberOfLogicalProcessors { get; set; }

    public string ProcessorId { get; set; } = string.Empty;

    public string SocketDesignation { get; set; } = string.Empty;

    public Boolean VirtualizationFirmwareEnabled { get; set; }

    public UInt64 PercentProcessorTime { get; set; }

    public List<CpuCore> CpuCoreList { get; set; } = new List<CpuCore>();
}