using System;

// https://docs.microsoft.com/en-us/windows/win32/cimwin32prov/win32-desktopmonitor
// https://learn.microsoft.com/en-us/windows/win32/wmicoreprov/wmimonitorid

namespace MacAgent.Components;

public class Monitor
{
    public string Vendor { get; set; } = string.Empty;

    public bool Main { get; set; }

    public bool Mirror { get; set; }

    public string MonitorType { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public ScreenResolution Resolution { get; set; }

    public bool Active { get; set; }

    public string ProductCodeID { get; set; } = string.Empty;

    public string SerialNumberID { get; set; } = string.Empty;

    public string ConnectionType { get; set; } = string.Empty;

    public ushort WeekOfManufacture { get; set; }

    public ushort YearOfManufacture { get; set; }
}

public readonly struct ScreenResolution
{
    public uint Width { get; }
    public uint Height { get; }

    public ScreenResolution(uint width, uint height)
    {
        Width = width;
        Height = height;
    }
}