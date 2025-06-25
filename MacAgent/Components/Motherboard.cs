using System;

// https://docs.microsoft.com/en-us/windows/win32/cimwin32prov/win32-baseboard

namespace MacAgent.Components;

public class Motherboard
{
    public string Manufacturer { get; set; } = string.Empty;

    public string Product { get; set; } = string.Empty;

    public string SerialNumber { get; set; } = string.Empty;
}