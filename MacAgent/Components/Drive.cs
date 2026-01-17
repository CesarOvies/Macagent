using System;
using System.Collections.Generic;

namespace MacAgent.Components;

// https://docs.microsoft.com/en-us/windows/win32/cimwin32prov/win32-logicaldisk

public class Volume
{
    public string Caption { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string FileSystem { get; set; } = string.Empty;

    public UInt64 FreeSpaceBytes { get; set; }

    public string Name { get; set; } = string.Empty;

    public UInt64 Size { get; set; }

    public string VolumeName { get; set; } = string.Empty;

    public string VolumeSerialNumber { get; set; } = string.Empty;
}

// https://docs.microsoft.com/en-us/windows/win32/cimwin32prov/win32-diskpartition

public class Partition
{
    public List<Volume> VolumeList { get; set; } = new List<Volume>();

    /// <summary>
    /// Indicates whether the computer can be booted from this partition.
    /// </summary>
    public Boolean Bootable { get; set; }

    /// <summary>
    /// Partition is the active partition. 
    /// The operating system uses the active partition when booting from a hard disk.
    /// </summary>
    public Boolean BootPartition { get; set; }

    public string Caption { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public UInt32 DiskIndex { get; set; }

    public UInt32 Index { get; set; }

    public string Name { get; set; } = string.Empty;

    public Boolean PrimaryPartition { get; set; }

    public UInt64 Size { get; set; }
}

// https://docs.microsoft.com/en-us/windows/win32/cimwin32prov/win32-diskdrive

public class Drive
{
    public List<Partition> PartitionList { get; set; } = new List<Partition>();

    public string MountPoint { get; set; } = string.Empty;

    public string FirmwareRevision { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public UInt32 Partitions { get; set; }

    public string SerialNumber { get; set; } = string.Empty;

    public ulong Size { get; set; }

    public bool IsRemovible { get; set; }
}