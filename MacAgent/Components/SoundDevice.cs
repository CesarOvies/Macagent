using System;

// https://docs.microsoft.com/en-us/windows/win32/cimwin32prov/win32-sounddevice

namespace MacAgent.Components;

public class SoundDevice
{
    public string Caption { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Manufacturer { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;
}