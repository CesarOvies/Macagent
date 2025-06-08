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

    public UInt32 PixelsPerXLogicalInch { get; set; }

    public UInt32 PixelsPerYLogicalInch { get; set; }

    public bool Active { get; set; }

    public string ManufacturerName { get; set; } = string.Empty;

    public string ProductCodeID { get; set; } = string.Empty;

    public string SerialNumberID { get; set; } = string.Empty;

    public string UserFriendlyName { get; set; } = string.Empty;

    public UInt16 WeekOfManufacture { get; set; }

    public UInt16 YearOfManufacture { get; set; }
}