using System;

// https://learn.microsoft.com/en-us/windows/win32/cimwin32prov/win32-computersystemproduct

namespace MacAgent.Components;

public class ComputerSystem
{
    public string Caption { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string SubType { get; set; } = string.Empty;

    public string IdentifyingNumber { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string SKUNumber { get; set; } = string.Empty;

    public string Vendor { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public string UUID { get; set; } = string.Empty;
}