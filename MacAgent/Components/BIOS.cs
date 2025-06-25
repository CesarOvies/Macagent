using System;

// https://docs.microsoft.com/en-us/windows/win32/cimwin32prov/win32-bios

namespace MacAgent.Components;

public class BIOS
{
    public string Caption { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Manufacturer { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string ReleaseDate { get; set; } = string.Empty;

    public string SerialNumber { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;
}