using System;

// https://docs.microsoft.com/en-us/windows/win32/cimwin32prov/win32-printer

namespace MacAgent.Components;

public class Printer
{
    public string Caption { get; set; } = string.Empty;

    public Boolean Default { get; set; }

    public string Description { get; set; } = string.Empty;

    public UInt32 HorizontalResolution { get; set; }

    public Boolean Local { get; set; }

    public string Name { get; set; } = string.Empty;

    public Boolean Network { get; set; }

    public Boolean Shared { get; set; }

    public UInt32 VerticalResolution { get; set; }
}