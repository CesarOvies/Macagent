using System;

namespace MacAgent.Components;

public class OS
{
    public string Name { get; set; } = string.Empty;

    public string VersionString { get; set; } = string.Empty;

    public Version Version { get; set; } = new Version();
}