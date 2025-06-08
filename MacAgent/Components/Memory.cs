using System;

// https://docs.microsoft.com/en-us/windows/win32/cimwin32prov/win32-physicalmemory

namespace MacAgent.Components;

public class Memory
{
    public string BankLabel { get; set; } = string.Empty;

    public UInt64 Capacity { get; set; }

    public FormFactor FormFactor { get; set; }

    public string Manufacturer { get; set; } = string.Empty;

    public UInt32 MaxVoltage { get; set; }

    public UInt32 MinVoltage { get; set; }

    public string PartNumber { get; set; } = string.Empty;

    public string SerialNumber { get; set; } = string.Empty;

    public UInt32 Speed { get; set; }
}

public enum FormFactor
{
    UNKNOWN = 0,

    OTHER = 1,

    SIP = 2,

    DIP = 3,

    ZIP = 4,

    SOJ = 5,

    PROPRIETARY = 6,

    SIMM = 7,

    DIMM = 8,

    TSOP = 9,

    PGA = 10,

    RIMM = 11,

    SODIMM = 12,

    SRIMM = 13,

    SMD = 14,

    SSMP = 15,

    QFP = 16,

    TQFP = 17,

    SOIC = 18,

    LCC = 19,

    PLCC = 20,

    BGA = 21,

    FPBGA = 22,

    LGA = 23
}