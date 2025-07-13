// https://docs.microsoft.com/en-us/windows/win32/cimwin32prov/win32-battery

namespace MacAgent.Components;

public class Battery
{
    public string SerialNumber { get; set; } = string.Empty;

    public string DeviceName { get; set; } = string.Empty;

    public string FirmwareVersion { get; set; } = string.Empty;

    public string HardwareRevision { get; set; } = string.Empty;

    public bool IsCharging { get; set; }

    public bool IsFullyCharged { get; set; }

    public ushort StateOfCharge { get; set; }

    public uint CycleCount { get; set; }

    public string Condition { get; set; } = string.Empty;

    public ushort MaximumCapacity { get; set; }

    public uint EstimatedRunTimeMinutes { get; set; }

    public uint TimeToFullChargeMinutes { get; set; }

    public string StatusDescription { get; set; } = string.Empty;

    public ACCharger ACCharger { get; set; } = new ACCharger();
}

public class ACCharger()
{
    public int ID { get; set; }

    public int Wattage { get; set; }

    public string Family { get; set; } = string.Empty;

    public string SerialNumber { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Manufacturer { get; set; } = string.Empty;

    public string HardwareVersion { get; set; } = string.Empty;

    public string FirmwareVersion { get; set; } = string.Empty;
};