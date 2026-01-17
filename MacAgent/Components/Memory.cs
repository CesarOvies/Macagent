using System;

// https://docs.microsoft.com/en-us/windows/win32/cimwin32prov/win32-physicalmemory

namespace MacAgent.Components;

public class Memory
{
    public UInt64 Capacity { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Manufacturer { get; set; } = string.Empty;

    public string PartNumber { get; set; } = string.Empty;

    public string SerialNumber { get; set; } = string.Empty;

    public UInt32 Speed { get; set; }
}