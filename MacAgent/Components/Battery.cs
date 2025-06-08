using System;

// https://docs.microsoft.com/en-us/windows/win32/cimwin32prov/win32-battery

namespace MacAgent.Components;

public class Battery
{
    public UInt32 FullChargeCapacity { get; set; }

    public UInt32 DesignCapacity { get; set; }

    public UInt16 BatteryStatus { get; set; }

    public UInt16 EstimatedChargeRemaining { get; set; }

    public UInt32 EstimatedRunTime { get; set; }

    public UInt32 ExpectedLife { get; set; }

    public UInt32 MaxRechargeTime { get; set; }

    public UInt32 TimeOnBattery { get; set; }

    public UInt32 TimeToFullCharge { get; set; }

    private string _batteryStatusDescription = string.Empty;

    public string BatteryStatusDescription
    {
        get => !string.IsNullOrEmpty(_batteryStatusDescription) ? _batteryStatusDescription : BatteryStatus switch
        {
            1 => "The battery is discharging",
            2 => "The system has access to AC so no battery is being discharged. However, the battery is not necessarily charging.",
            3 => "Fully Charged",
            4 => "Low",
            5 => "Critical",
            6 => "Charging",
            7 => "Charging and High",
            8 => "Charging and Low",
            9 => "Charging and Critical",
            10 => "No battery is installed",
            11 => "Partially Charged",
            _ => string.Empty
        };

        set => _batteryStatusDescription = value;
    }
}