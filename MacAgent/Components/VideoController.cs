using System;

// https://docs.microsoft.com/en-us/windows/win32/cimwin32prov/win32-videocontroller

namespace MacAgent.Components;

public class VideoController
{
    public UInt64 AdapterRAM { get; set; }

    public string Caption { get; set; } = string.Empty;

    public UInt32 CurrentBitsPerPixel { get; set; }

    public UInt32 CurrentHorizontalResolution { get; set; }

    public UInt64 CurrentNumberOfColors { get; set; }

    public UInt32 CurrentRefreshRate { get; set; }

    public UInt32 CurrentVerticalResolution { get; set; }

    public string Description { get; set; } = string.Empty;

    public string DriverDate { get; set; } = string.Empty;

    public string DriverVersion { get; set; } = string.Empty;

    public string Manufacturer { get; set; } = string.Empty;

    public UInt32 MaxRefreshRate { get; set; }

    public UInt32 MinRefreshRate { get; set; }

    public string Name { get; set; } = string.Empty;

    public string VideoModeDescription { get; set; } = string.Empty;

    public string VideoProcessor { get; set; } = string.Empty;
}