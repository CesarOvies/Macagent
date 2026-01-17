using System.Data;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using MacAgent.Components;

namespace MacAgent.Services;

public class HardwareInfo
{
    private static string GetValueFromKey(XElement? dictNode, string key)
    {
        XElement? keyElement = dictNode?.Elements("key").FirstOrDefault(k => k.Value == key);

        if (keyElement != null && keyElement.NextNode is XElement valueElement)
        {
            if (valueElement.Name == "true" || valueElement.Name == "false")
            {
                return valueElement.Name.LocalName;
            }

            return valueElement.Value;
        }
        return string.Empty;
    }

    private static ulong ExtractSizeInBytes(string input)
    {
        if (string.IsNullOrEmpty(input)) return 0;

        Match match = Regex.Match(input, @"(\d+)\s*([KMGT]B)", RegexOptions.IgnoreCase);

        if (!match.Success || !ulong.TryParse(match.Groups[1].Value, out ulong size))
        {
            return 0;
        }

        Dictionary<string, ulong> multipliers = new Dictionary<string, ulong>
    {
        { "KB", 1024UL },
        { "MB", 1024UL * 1024 },
        { "GB", 1024UL * 1024 * 1024 },
        { "TB", 1024UL * 1024 * 1024 * 1024 }
    };

        string unit = match.Groups[2].Value.ToUpper();
        return multipliers.TryGetValue(unit, out ulong multiplier) ? size * multiplier : size;
    }

    public static ComputerSystem GetComputer()
    {
        ComputerSystem computerSystem = new ComputerSystem { Vendor = "Apple Inc." };

        try
        {
            string xmlOutput = ProcessInfo.ReadProcessOut("system_profiler", "SPHardwareDataType -xml");
            if (string.IsNullOrWhiteSpace(xmlOutput)) return computerSystem;

            XDocument doc = XDocument.Parse(xmlOutput);
            XElement? items = doc.Descendants("key").FirstOrDefault(k => k.Value == "_items")?.NextNode as XElement;
            XElement? hardwareInfo = items?.Elements("dict").FirstOrDefault();

            if (hardwareInfo != null)
            {
                string modelName = GetValueFromKey(hardwareInfo, "machine_name") ?? "Unknown";
                computerSystem.Caption = modelName;
                computerSystem.Name = modelName;
                computerSystem.SubType = GetComputerSubType(modelName);
                computerSystem.Description = GetValueFromKey(hardwareInfo, "machine_model");
                computerSystem.IdentifyingNumber = GetValueFromKey(hardwareInfo, "serial_number");
                computerSystem.UUID = GetValueFromKey(hardwareInfo, "platform_UUID");
                computerSystem.SKUNumber = GetValueFromKey(hardwareInfo, "model_number");
                computerSystem.Version = GetValueFromKey(hardwareInfo, "boot_rom_version");
            }
        }
        catch (Exception)
        {
        }

        return computerSystem;
    }

    private static string GetComputerSubType(string model_name)
    {
        if (model_name.Contains("MacBook", StringComparison.OrdinalIgnoreCase))
            return "Laptop";
        else if (model_name.Contains("iMac", StringComparison.OrdinalIgnoreCase) ||
                 model_name.Contains("Mac mini", StringComparison.OrdinalIgnoreCase) ||
                 model_name.Contains("Mac Pro", StringComparison.OrdinalIgnoreCase) ||
                 model_name.Contains("Mac Studio", StringComparison.OrdinalIgnoreCase))
            return "Desktop";
        else
            return "Other (Apple Device)";
    }

    public static List<CPU> GetCpu()
    {
        string brand_string = ProcessInfo.ReadProcessOut("sysctl", "-n machdep.cpu.brand_string");
        string cpu_name = brand_string.Split('@')[0].Trim();
        string nperf_levels = ProcessInfo.ReadProcessOut("sysctl", "-n hw.nperflevels");
        uint.TryParse(nperf_levels, out uint nperf_levels_out);
        List<CPU> cpu_list = [];

        if (nperf_levels_out > 1)
        {
            for (int i = 0; i < nperf_levels_out; i++)
            {
                CPU cpu = new CPU
                {
                    Name = cpu_name,
                    Caption = i.ToString(),
                    Description = $"perflevel{i}"
                };

                PopulateCpuDetails(cpu, $"hw.perflevel{i}.");
                cpu_list.Add(cpu);
            }
        }
        else
        {
            CPU cpu = new CPU { Name = cpu_name };
            PopulateCpuDetails(cpu, "hw.");
            cpu_list.Add(cpu);
        }

        return cpu_list;
    }

    /// <param name="cpu">O objeto CPU a ser preenchido.</param>
    /// <param name="key_prefix">O prefixo a ser usado para as chaves sysctl ("hw." ou "hw.perflevelX.").</param>
    private static void PopulateCpuDetails(CPU cpu, string key_prefix)
    {
        uint GetSysctlUint(string key)
        {
            string output = ProcessInfo.ReadProcessOut("sysctl", $"-n {key}");
            uint.TryParse(output, out uint value);
            return value;
        }

        cpu.MaxClockSpeed = GetSysctlUint("hw.cpufrequency_max") / 1_000_000;
        cpu.CurrentClockSpeed = GetSysctlUint("hw.cpufrequency") / 1_000_000;
        cpu.L1InstructionCacheSize = GetSysctlUint($"{key_prefix}l1icachesize");
        cpu.L1DataCacheSize = GetSysctlUint($"{key_prefix}l1dcachesize");
        cpu.L2CacheSize = GetSysctlUint($"{key_prefix}l2cachesize");
        cpu.L3CacheSize = GetSysctlUint($"{key_prefix}l3cachesize");
        cpu.NumberOfCores = GetSysctlUint($"{key_prefix}physicalcpu");
        cpu.NumberOfLogicalProcessors = GetSysctlUint($"{key_prefix}logicalcpu");
    }

    public static List<Battery> GetBattery()
    {
        Battery battery = new Battery();

        try
        {
            string xmlOutput = ProcessInfo.ReadProcessOut("system_profiler", "SPPowerDataType -xml");

            if (string.IsNullOrWhiteSpace(xmlOutput))
            {
                return [battery];
            }

            XDocument doc = XDocument.Parse(xmlOutput);
            IEnumerable<XElement>? items = (doc.Descendants("key").FirstOrDefault(k => k.Value == "_items")?.NextNode as XElement)?.Elements("dict");

            if (items != null)
            {
                XElement? batteryNode = items.FirstOrDefault(d => GetValueFromKey(d, "_name") == "spbattery_information");

                if (batteryNode != null)
                {
                    if (batteryNode.Elements("key").FirstOrDefault(k => k.Value == "sppower_battery_charge_info")?.NextNode is XElement chargeInfo)
                    {
                        if (ushort.TryParse(GetValueFromKey(chargeInfo, "sppower_battery_state_of_charge"), out ushort charge))
                        {
                            battery.StateOfCharge = charge;
                        }
                        battery.IsCharging = GetValueFromKey(chargeInfo, "sppower_battery_is_charging")?.ToUpper() == "TRUE";
                    }

                    if (batteryNode.Elements("key").FirstOrDefault(k => k.Value == "sppower_battery_health_info")?.NextNode is XElement healthInfo)
                    {
                        if (uint.TryParse(GetValueFromKey(healthInfo, "sppower_battery_cycle_count"), out uint cycles))
                        {
                            battery.CycleCount = cycles;
                        }
                        battery.Condition = GetValueFromKey(healthInfo, "sppower_battery_health");
                        string? maxCapacityStr = GetValueFromKey(healthInfo, "sppower_battery_health_maximum_capacity")?.Replace("%", "");

                        if (ushort.TryParse(maxCapacityStr, out ushort maxCapacity))
                        {
                            battery.MaximumCapacity = maxCapacity;
                        }
                    }

                    if (batteryNode.Elements("key").FirstOrDefault(k => k.Value == "sppower_battery_model_info")?.NextNode is XElement modelInfo)
                    {
                        battery.SerialNumber = GetValueFromKey(modelInfo, "sppower_battery_serial_number");
                        battery.DeviceName = GetValueFromKey(modelInfo, "sppower_battery_device_name");
                        battery.FirmwareVersion = GetValueFromKey(modelInfo, "sppower_battery_firmware_version");
                        battery.HardwareRevision = GetValueFromKey(modelInfo, "sppower_battery_hardware_revision");
                    }
                }

                XElement? chargerInfo = items.FirstOrDefault(d => GetValueFromKey(d, "_name") == "sppower_ac_charger_information");

                if (chargerInfo != null)
                {
                    bool charger_connected = GetValueFromKey(chargerInfo, "sppower_battery_charger_connected")?.ToUpper() == "TRUE";

                    if (charger_connected)
                    {
                        int.TryParse(GetValueFromKey(chargerInfo, "sppower_ac_charger_ID").Replace("0x", ""),
                        System.Globalization.NumberStyles.HexNumber,
                        System.Globalization.CultureInfo.InvariantCulture, out int id);
                        int.TryParse(GetValueFromKey(chargerInfo, "sppower_ac_charger_watts"), out int wattage);

                        battery.ACCharger = new ACCharger()
                        {
                            Name = GetValueFromKey(chargerInfo, "sppower_ac_charger_name"),
                            SerialNumber = GetValueFromKey(chargerInfo, "sppower_ac_charger_serial_number"),
                            Family = GetValueFromKey(chargerInfo, "sppower_ac_charger_family"),
                            Manufacturer = GetValueFromKey(chargerInfo, "sppower_ac_charger_manufacturer"),
                            HardwareVersion = GetValueFromKey(chargerInfo, "sppower_ac_charger_hardware_version"),
                            FirmwareVersion = GetValueFromKey(chargerInfo, "sppower_ac_charger_firmware_version"),
                            ID = id,
                            Wattage = wattage
                        };
                    }
                }
            }
        }
        catch (Exception)
        {
        }

        string pmset_output = ProcessInfo.ReadProcessOut("pmset", "-g batt");
        Match status_match = Regex.Match(pmset_output, @"'(.*?)'");

        if (status_match.Success)
        {
            battery.StatusDescription = status_match.Groups[1].Value;
        }

        Match time_match = Regex.Match(pmset_output, @"(\d+:\d+)\s*(remaining|to charge)");

        if (time_match.Success && TimeSpan.TryParse(time_match.Groups[1].Value, out TimeSpan time_span))
        {
            uint total_minutes = (uint)time_span.TotalMinutes;

            if (time_match.Groups[2].Value == "remaining")
            {
                battery.EstimatedRunTimeMinutes = total_minutes;
            }
            else if (time_match.Groups[2].Value == "to charge")
            {
                battery.TimeToFullChargeMinutes = total_minutes;
            }
        }

        return [battery];
    }

    public static List<Drive> GetDrive()
    {
        Dictionary<string, Drive> drivesDictionary = [];

        try
        {
            List<string> physicalDriveDataTypes = ["SPNVMeDataType", "SPSerialATADataType"];

            foreach (string dataType in physicalDriveDataTypes)
            {
                string xmlOutput = ProcessInfo.ReadProcessOut("system_profiler", $"{dataType} -xml");
                if (string.IsNullOrWhiteSpace(xmlOutput)) continue;

                XDocument doc = XDocument.Parse(xmlOutput);
                IEnumerable<XElement>? controllerNodes = (doc.Descendants("key").FirstOrDefault(k => k.Value == "_items")?.NextNode as XElement)?.Elements("dict");
                if (controllerNodes == null) continue;

                foreach (XElement cNode in controllerNodes)
                {
                    IEnumerable<XElement>? driveNodes = (cNode.Elements("key").FirstOrDefault(k => k.Value == "_items")?.NextNode as XElement)?.Elements("dict");
                    if (driveNodes == null) continue;

                    foreach (XElement pNode in driveNodes)
                    {
                        string model = GetValueFromKey(pNode, "device_model") ?? GetValueFromKey(pNode, "_name") ?? "Unknown";

                        if (drivesDictionary.ContainsKey(model)) continue;

                        Drive drive = new Drive
                        {
                            Model = model,
                            Name = GetValueFromKey(pNode, "_name"),
                            MountPoint = GetValueFromKey(pNode, "bsd_name"),
                            IsRemovible = GetValueFromKey(pNode, "removable_media").Equals("yes", StringComparison.CurrentCultureIgnoreCase),
                            SerialNumber = GetValueFromKey(pNode, "device_serial"),
                            FirmwareRevision = GetValueFromKey(pNode, "device_revision")
                        };

                        ulong.TryParse(GetValueFromKey(pNode, "size_in_bytes"), out ulong size);
                        drive.Size = size;

                        drivesDictionary.TryAdd(model, drive);
                    }
                }
            }
            string storageXmlOutput = ProcessInfo.ReadProcessOut("system_profiler", "SPStorageDataType -xml");

            if (!string.IsNullOrWhiteSpace(storageXmlOutput))
            {
                XDocument storageDoc = XDocument.Parse(storageXmlOutput);
                IEnumerable<XElement>? volumeNodes = (storageDoc.Descendants("key").FirstOrDefault(k => k.Value == "_items")?.NextNode as XElement)?.Elements("dict");

                if (volumeNodes != null)
                {
                    foreach (XElement vNode in volumeNodes)
                    {
                        if (vNode.Elements("key").FirstOrDefault(k => k.Value == "physical_drive")?.NextNode is not XElement physicalDriveNode) continue;

                        string physicalDriveModel = GetValueFromKey(physicalDriveNode, "device_name") ?? "Unknown Drive";

                        if (!drivesDictionary.TryGetValue(physicalDriveModel, out Drive? parentDrive))
                        {
                            parentDrive = new Drive { Model = physicalDriveModel };
                            drivesDictionary.Add(physicalDriveModel, parentDrive);
                        }

                        Volume volume = new Volume
                        {
                            VolumeName = GetValueFromKey(vNode, "_name"),
                            Name = GetValueFromKey(vNode, "_name"),
                            FileSystem = GetValueFromKey(vNode, "file_system"),
                            VolumeSerialNumber = GetValueFromKey(vNode, "volume_uuid"),
                        };

                        ulong.TryParse(GetValueFromKey(vNode, "size_in_bytes"), out ulong volSize);
                        volume.Size = volSize;
                        ulong.TryParse(GetValueFromKey(vNode, "free_space_in_bytes"), out ulong volFree);
                        volume.FreeSpaceBytes = volFree;

                        Partition partition = new Partition
                        {
                            Caption = GetValueFromKey(vNode, "bsd_name"),
                            Name = GetValueFromKey(vNode, "bsd_name"),
                            Size = volSize,
                            BootPartition = GetValueFromKey(vNode, "mount_point") == "/"
                        };

                        partition.VolumeList.Add(volume);
                        parentDrive.PartitionList.Add(partition);
                        parentDrive.Partitions++;
                    }
                }
            }
        }
        catch (Exception)
        {
        }

        return drivesDictionary.Values.ToList();
    }

    public static List<Keyboard> GetKeyboard()
    {
        List<Keyboard> keyboards = [];
        try
        {
            string xmlOutput = ProcessInfo.ReadProcessOut("system_profiler", "SPHIDDeviceDataType -xml");
            if (string.IsNullOrWhiteSpace(xmlOutput)) return keyboards;

            XDocument doc = XDocument.Parse(xmlOutput);
            IEnumerable<XElement>? items = (doc.Descendants("key").FirstOrDefault(k => k.Value == "_items")?.NextNode as XElement)?.Elements("dict");

            if (items != null)
            {
                keyboards = items
                    .Where(d => (GetValueFromKey(d, "product") ?? "").Contains("Keyboard", StringComparison.OrdinalIgnoreCase))
                    .Select(d => new Keyboard
                    {
                        Caption = GetValueFromKey(d, "product"),
                        Description = GetValueFromKey(d, "product"),
                        Name = GetValueFromKey(d, "_name")
                    })
                    .ToList();
            }
        }
        catch (Exception)
        {
        }
        return keyboards;
    }

    public static List<Memory> GetMemory()
    {
        List<Memory> memory_list = [];
        try
        {
            string xmlOutput = ProcessInfo.ReadProcessOut("system_profiler", "SPMemoryDataType -xml");
            if (string.IsNullOrWhiteSpace(xmlOutput)) return memory_list;

            XDocument doc = XDocument.Parse(xmlOutput);
            XElement? rootDict = doc.Root?.Element("array")?.Element("dict");
            if (rootDict == null) return memory_list;

            XElement? itemsKey = rootDict.Elements("key").FirstOrDefault(k => k.Value == "_items");

            if (itemsKey != null && itemsKey.NextNode is XElement memoryArray)
            {
                IEnumerable<XElement> memoryModules = memoryArray.Elements("dict");

                foreach (XElement node in memoryModules)
                {
                    Memory memory = new Memory
                    {
                        Capacity = ExtractSizeInBytes(GetValueFromKey(node, "dimm_size") ?? "0"),
                        Manufacturer = GetValueFromKey(node, "dimm_manufacturer"),
                        PartNumber = GetValueFromKey(node, "dimm_part_number"),
                        SerialNumber = GetValueFromKey(node, "dimm_serial_number"),
                        Type = GetValueFromKey(node, "dimm_type")
                    };

                    string? speedStr = GetValueFromKey(node, "dimm_speed");
                    if (!string.IsNullOrEmpty(speedStr) && uint.TryParse(speedStr.Split(' ')[0], out uint speed))
                    {
                        memory.Speed = speed;
                    }

                    memory_list.Add(memory);
                }
            }
        }
        catch (Exception)
        {
        }

        return memory_list;
    }

    public static List<Components.Monitor> GetMonitor()
    {
        List<Components.Monitor> monitor_list = [];

        try
        {
            string read_process_out = ProcessInfo.ReadProcessOut("system_profiler", "SPDisplaysDataType -xml");

            if (string.IsNullOrWhiteSpace(read_process_out))
            {
                return monitor_list;
            }

            XDocument doc = XDocument.Parse(read_process_out);
            XElement? items = doc.Descendants("key").FirstOrDefault(k => k.Value == "_items");

            if (items != null && items.NextNode is XElement items_array)
            {
                foreach (XElement node in items_array.Elements("dict"))
                {
                    XElement? displays_keys = node.Elements("key").FirstOrDefault(k => k.Value == "spdisplays_ndrvs");

                    if (displays_keys != null && displays_keys.NextNode is XElement displays_array)
                    {
                        foreach (XElement monitor_node in displays_array.Elements("dict"))
                        {
                            Components.Monitor monitor = new()
                            {
                                Name = GetValueFromKey(monitor_node, "_name") ?? "Unknown",
                                Vendor = GetVendorNameFromId(GetValueFromKey(monitor_node, "_spdisplays_display-vendor-id")),
                                MonitorType = GetValueFromKey(monitor_node, "spdisplays_display_type").Replace("spdisplays_", ""),
                                SerialNumberID = GetValueFromKey(monitor_node, "_spdisplays_display-serial-number"),
                                ProductCodeID = GetValueFromKey(monitor_node, "_spdisplays_display-product-id"),
                                Main = (GetValueFromKey(monitor_node, "spdisplays_main") ?? "no") == "spdisplays_yes",
                                Active = (GetValueFromKey(monitor_node, "spdisplays_online") ?? "no") == "spdisplays_yes",
                                Mirror = (GetValueFromKey(monitor_node, "spdisplays_mirror") ?? "off") == "spdisplays_on",
                                WeekOfManufacture = ushort.TryParse(GetValueFromKey(monitor_node, "_spdisplays_display-week"), out ushort week) ? week : (ushort)0,
                                YearOfManufacture = ushort.TryParse(GetValueFromKey(monitor_node, "_spdisplays_display-year"), out ushort year) ? year : (ushort)0,
                            };

                            string connection_type = GetValueFromKey(monitor_node, "spdisplays_connection_type").Replace("spdisplays_", "");

                            if (string.IsNullOrWhiteSpace(connection_type))
                            {
                                connection_type = "external";
                            }

                            monitor.ConnectionType = connection_type;

                            string resolution = GetValueFromKey(monitor_node, "_spdisplays_resolution");

                            if (string.IsNullOrEmpty(resolution) == false)
                            {
                                string[] dimensions = Regex.Matches(resolution, @"\d+").Cast<Match>().Select(m => m.Value).ToArray();

                                if (dimensions.Length >= 2 &&
                                    uint.TryParse(dimensions[0], out uint width) &&
                                    uint.TryParse(dimensions[1], out uint height))
                                {
                                    monitor.Resolution = new ScreenResolution(width, height);
                                }
                            }

                            monitor_list.Add(monitor);
                        }
                    }
                }
            }
        }
        catch (Exception)
        {
        }

        return monitor_list;
    }

    private static string GetVendorNameFromId(string vendor_id)
    {
        if (VendorIdMap.TryGetValue(vendor_id, out string? vendor_name))
        {
            return vendor_name;
        }

        return vendor_id;
    }

    private static readonly Dictionary<string, string> VendorIdMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "0610", "Apple Inc." },
        { "610", "Apple Inc." },
        { "1025", "Acer" },
        { "1043", "ASUS" },
        { "10ac", "Dell" },
        { "1e6d", "LG Electronics" },
        { "15c3", "ViewSonic" },
        { "1946", "BenQ" },
        { "19ac", "Eizo" },
        { "22f0", "HP Inc." },
        { "3023", "Lenovo" },
        { "38a3", "NEC" },
        { "4c2d", "Samsung" },
        { "0471", "Philips" },
        { "05a3", "AOC" },
        { "1002", "AMD (Advanced Micro Devices, Inc.)" },
        { "10de", "NVIDIA Corporation" },
        { "1102", "Creative Labs" },
        { "1458", "GIGABYTE" },
        { "1462", "MSI (Micro-Star International)" },
        { "1b1c", "Corsair" },
        { "8086", "Intel Corporation" },
        { "10ec", "Realtek Semiconductor Corp." },
        { "046d", "Logitech" },
        { "104d", "Sony Corporation" },
        { "1179", "Toshiba" },
        { "1414", "Microsoft Corporation" },
        { "1532", "Razer Inc." },
        { "10f7", "Panasonic" }
    };

    public static Motherboard GetMotherboard()
    {
        Motherboard motherboard = new()
        {
            Manufacturer = "Apple Inc."
        };

        try
        {
            string xml_output = ProcessInfo.ReadProcessOut("system_profiler", "SPHardwareDataType -xml");

            if (string.IsNullOrWhiteSpace(xml_output))
            {
                return motherboard;
            }

            XDocument doc = XDocument.Parse(xml_output);

            XElement? hardware_info = (doc.Descendants("key")
                .FirstOrDefault(k => k.Value == "_items")?.NextNode as XElement)?
                .Elements("dict")
                .FirstOrDefault();

            if (hardware_info != null)
            {
                motherboard.Product = GetValueFromKey(hardware_info, "machine_model") ?? "Unknown";
                motherboard.SerialNumber = GetValueFromKey(hardware_info, "serial_number") ?? "Unknown";
            }
        }
        catch (Exception)
        {
        }

        return motherboard;
    }

    public static List<Mouse> GetMouse()
    {
        List<Mouse> mouseList = new List<Mouse>();

        string[] profilerDataTypes = { "SPSPIDataType", "SPBluetoothDataType" };
        foreach (string dataType in profilerDataTypes)
        {
            try
            {
                string xmlOutput = ProcessInfo.ReadProcessOut("system_profiler", $"{dataType} -xml");
                if (!string.IsNullOrWhiteSpace(xmlOutput))
                {
                    ParseProfilerXml(xmlOutput, dataType, mouseList);
                }
            }
            catch
            {
            }
        }

        try
        {
            string mappingXml = ProcessInfo.ReadProcessOut("ioreg", "-a -p IOUSB");

            if (!string.IsNullOrWhiteSpace(mappingXml))
            {
                XDocument mappingDoc = XDocument.Parse(mappingXml);

                List<string> deviceNames = [.. mappingDoc.Descendants("dict")
                    .Select(d => GetValueFromKey(d, "IORegistryEntryName") ?? GetValueFromKey(d, "_name"))
                    .Where(n => !string.IsNullOrEmpty(n))
                    .Distinct()];

                foreach (string name in deviceNames)
                {
                    if (IsPotentialMouse(name))
                    {
                        try
                        {
                            string detailXml = ProcessInfo.ReadProcessOut("ioreg", $"-n \"{name}\" -r -a");

                            if (!string.IsNullOrWhiteSpace(detailXml))
                            {
                                ParseDetailedUsbXml(detailXml, mouseList);
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }
        }
        catch (Exception)
        {
        }

        return mouseList;
    }

    private static bool IsPotentialMouse(string name)
    {
        return name.Contains("Mouse", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("Trackpad", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("Wireless Device", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("Receiver", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("Magic", StringComparison.OrdinalIgnoreCase);
    }

    private static void ParseDetailedUsbXml(string xml, List<Mouse> list)
    {
        try
        {
            XDocument doc = XDocument.Parse(xml);

            foreach (XElement dict in doc.Descendants("dict"))
            {
                string name = GetValueFromKey(dict, "USB Product Name") ??
                              GetValueFromKey(dict, "IORegistryEntryName") ?? "";

                if (!IsPotentialMouse(name))
                {
                    continue;
                }

                string manufacturer = GetValueFromKey(dict, "USB Vendor Name") ??
                                      GetValueFromKey(dict, "manufacturer") ??
                                      "Unknown";

                if (manufacturer == "Unknown" || string.IsNullOrEmpty(manufacturer))
                {
                    string vid = GetValueFromKey(dict, "idVendor");

                    if (!string.IsNullOrEmpty(vid)) manufacturer = GetVendorNameFromId(vid);
                }

                if (!list.Any(m => m.Name == name))
                {
                    list.Add(new Mouse
                    {
                        Name = name,
                        Caption = name,
                        Description = "USB Device",
                        Manufacturer = manufacturer,
                        NumberOfButtons = (byte)(name.Contains("Trackpad", StringComparison.OrdinalIgnoreCase) ? 1 : 2)
                    });
                }
            }
        }
        catch { }
    }

    private static void ParseProfilerXml(string xml, string dataType, List<Mouse> list)
    {
        XDocument doc = XDocument.Parse(xml);

        if (dataType == "SPBluetoothDataType")
        {
            if (doc.Descendants("key").FirstOrDefault(k => k.Value == "device_connected")?.NextNode is XElement connected)
            {
                foreach (XElement wrapper in connected.Elements("dict"))
                {
                    XElement? key = wrapper.Elements("key").FirstOrDefault();

                    if (key != null)
                    {
                        string name = key.Value;
                        XElement? details = key.NextNode as XElement;
                        string type = GetValueFromKey(details, "device_minorType") ?? "";

                        if (IsPotentialMouse(name) ||
                            type.Contains("Mouse") ||
                            type.Contains("Trackpad"))
                        {
                            string vendor = GetValueFromKey(details, "device_vendorID");
                            if (!list.Any(m => m.Name == name))
                            {
                                list.Add(new Mouse
                                {
                                    Name = name,
                                    Caption = name,
                                    Description = "Bluetooth",
                                    Manufacturer = GetVendorNameFromId(vendor?.Replace("0x", "") ?? ""),
                                    NumberOfButtons = 2
                                });
                            }
                        }
                    }
                }
            }
        }
        else
        {
            foreach (XElement dict in doc.Descendants("dict"))
            {
                string name = GetValueFromKey(dict, "_name") ?? "";

                if (name.Contains("Trackpad"))
                {
                    if (!list.Any(m => m.Name == name))
                        list.Add(new Mouse
                        {
                            Name = name,
                            Caption = name,
                            Description = "SPI",
                            Manufacturer = "Apple Inc.",
                            NumberOfButtons = 1
                        });
                }
            }
        }
    }
}