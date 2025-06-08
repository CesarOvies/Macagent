using System;

// https://docs.microsoft.com/en-us/windows/win32/api/sysinfoapi/ns-sysinfoapi-memorystatusex

namespace MacAgent.Components;

public class MemoryStatus
{
    public ulong TotalPhysical { get; set; }

    public ulong AvailablePhysical { get; set; }

    public ulong TotalPageFile { get; set; }

    public ulong AvailablePageFile { get; set; }

    public ulong TotalVirtual { get; set; }

    public ulong AvailableVirtual { get; set; }

    public ulong AvailableExtendedVirtual { get; set; }
}