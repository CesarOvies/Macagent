using System;

// https://docs.microsoft.com/en-us/windows/win32/cimwin32prov/win32-desktopmonitor
// https://learn.microsoft.com/en-us/windows/win32/wmicoreprov/wmimonitorid

namespace MacAgent.Components;

public class Monitor
{
    public string Caption { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string MonitorManufacturer { get; set; } = string.Empty;

    public string MonitorType { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public ScreenResolution Resolution { get; set; }

    public bool Active { get; set; }

    public string ManufacturerName { get; set; } = string.Empty;

    public string ProductCodeID { get; set; } = string.Empty;

    public string SerialNumberID { get; set; } = string.Empty;

    public string UserFriendlyName { get; set; } = string.Empty;

    public UInt16 WeekOfManufacture { get; set; }

    public UInt16 YearOfManufacture { get; set; }
}

public struct ScreenResolution
{
    public uint Width { get; }
    public uint Height { get; }

    public ScreenResolution(uint width, uint height)
    {
        Width = width;
        Height = height;
    }

    public override readonly string ToString() => $"{Width}x{Height}";
}