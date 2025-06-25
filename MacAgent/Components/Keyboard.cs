using System;

// https://docs.microsoft.com/en-us/windows/win32/cimwin32prov/win32-keyboard

namespace MacAgent.Components;

public class Keyboard
{
    public string Caption { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public UInt16 NumberOfFunctionKeys { get; set; }
}
