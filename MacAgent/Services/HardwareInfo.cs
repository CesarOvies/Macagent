using System.Data;
using System.Text.RegularExpressions;
using MacAgent.Components;

namespace MacAgent.Services;

public class HardwareInfo
{
    public static ComputerSystem GetComputer()
    {
        ComputerSystem computer_system = new ComputerSystem
        {
            Vendor = "Apple"
        };

        try
        {
            List<string> lines = [.. ProcessInfo.ReadProcessOut("system_profiler", "SPHardwareDataType").Split('\n', StringSplitOptions.TrimEntries)];

            foreach (string line in lines)
            {
                if (line.StartsWith("Model Name: "))
                {
                    string _line = line.Replace("Model Name: ", string.Empty);
                    computer_system.Caption = _line;
                    computer_system.Name = _line;
                    computer_system.SubType = GetComputerSubType(_line);
                }
                else if (line.StartsWith("Model Identifier: "))
                {
                    computer_system.Description = line.Replace("Model Identifier: ", string.Empty);
                }
                else if (line.StartsWith("Serial Number (system): "))
                {
                    computer_system.IdentifyingNumber = line.Replace("Serial Number (system): ", string.Empty);
                }
                else if (line.StartsWith("Hardware UUID"))
                {
                    computer_system.UUID = line.Replace("Hardware UUID: ", string.Empty);
                }
                else if (line.StartsWith("Model Number: "))
                {
                    computer_system.SKUNumber = line.Replace("Model Number: ", string.Empty);
                }
                else if (line.StartsWith("System Firmware Version: "))
                {
                    computer_system.Version = line.Replace("System Firmware Version: ", string.Empty);
                }
            }
        }
        catch (Exception)
        {
        }

        return computer_system;
    }

    private static string GetComputerSubType(string model_name)
    {
        if (model_name.Contains("MacBook", StringComparison.OrdinalIgnoreCase))
            return "Laptop";
        else if (model_name.Contains("iMac", StringComparison.OrdinalIgnoreCase) ||
                 model_name.Contains("Mac mini", StringComparison.OrdinalIgnoreCase) ||
                 model_name.Contains("Mac Pro", StringComparison.OrdinalIgnoreCase))
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
        List<CPU> cpu_list = new List<CPU>();

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
        string profiler_output = ProcessInfo.ReadProcessOut("system_profiler", "SPPowerDataType");
        string[] lines = profiler_output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        Dictionary<string, Action<string>> battery_property_map = new Dictionary<string, Action<string>>
    {
        { "Serial Number", value => battery.SerialNumber = value },
        { "Device Name", value => battery.DeviceName = value },
        { "Firmware Version", value => battery.FirmwareVersion = value },
        { "Hardware Revision", value => battery.HardwareRevision = value },
        { "Fully Charged", value => battery.IsFullyCharged = value.Equals("Yes", StringComparison.OrdinalIgnoreCase) },
        { "Charging", value => battery.IsCharging = value.Equals("Yes", StringComparison.OrdinalIgnoreCase) },
        { "State of Charge (%)", value =>
            {
                string porcentage = value.Substring(value.IndexOf(':') + 1).Trim();

                if(ushort.TryParse(porcentage, out ushort parse))
                {
                    battery.StateOfCharge = parse;
                }
            }
        },
        { "Cycle Count", value =>
            {
                string numeric_part = value.Substring(value.IndexOf(':') + 1).Trim().TrimEnd('%');

                if (uint.TryParse(numeric_part, out uint parse))
                {
                    battery.CycleCount = parse;
                }
            }
        },
        { "Condition", value => battery.Condition = value },
        { "Maximum Capacity", value =>
            {
                string numeric_part = value.Substring(value.IndexOf(':') + 1).Trim().TrimEnd('%');

                if (ushort.TryParse(numeric_part, out ushort capacity))
                {
                    battery.MaximumCapacity = capacity;
                }
            }
        }
    };

        Dictionary<string, Action<string>> accharger_property_map = new Dictionary<string, Action<string>>
    {
        { "Wattage (W)", value =>
            {
                if (int.TryParse(value, out int wattage))
                {
                    battery.ACCharger.Wattage = wattage;
                }
            }
        },
        { "Name", value => battery.ACCharger.Name = value },
        { "Manufacturer", value => battery.ACCharger.Manufacturer = value },
        { "Serial Number", value => battery.ACCharger.SerialNumber = value },
        { "Hardware Version", value => battery.ACCharger.HardwareVersion = value },
        { "Firmware Version", value => battery.ACCharger.FirmwareVersion = value },
        { "Family", value => battery.ACCharger.Family = value },
        { "ID", value =>
            {
                if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    battery.ACCharger.ID = int.TryParse(value.Substring(2),
                                                        System.Globalization.NumberStyles.HexNumber,
                                                        null, out int id) ? id : 0;
                }
            }
        }
    };
        bool parsing_accharger = false;

        foreach (string line in lines)
        {
            string trimmed_line = line.Trim();

            if (trimmed_line.Equals("AC Charger Information:", StringComparison.OrdinalIgnoreCase))
            {
                parsing_accharger = true;
                continue;
            }
            else if (parsing_accharger && (string.IsNullOrWhiteSpace(trimmed_line) || !trimmed_line.Contains(":")))
            {
                parsing_accharger = false;
                continue;
            }

            string[] parts = trimmed_line.Split(new[] { ':' }, 2);

            if (parts.Length != 2)
            {
                continue;
            }

            string key = parts[0].Trim();
            string value = parts[1].Trim();

            if (parsing_accharger)
            {
                if (accharger_property_map.TryGetValue(key, out Action<string>? ac_action))
                {
                    ac_action(value);
                }
            }
            else
            {
                if (battery_property_map.TryGetValue(key, out Action<string>? battery_action))
                {
                    battery_action(value);
                }
            }
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

        return new List<Battery> { battery };
    }

    public static List<Drive> GetDrive()
    {
        string process_out = ProcessInfo.ReadProcessOut("lshw", "-class disk");
        string[] device_blocks = process_out.Split(new[] { "*-" }, StringSplitOptions.RemoveEmptyEntries);
        List<Drive> drives = new List<Drive>();

        foreach (string block in device_blocks)
        {
            if (!block.Trim().StartsWith("disk") && !block.Trim().StartsWith("cdrom"))
            {
                continue;
            }

            Drive drive = new Drive();
            string[] lines = block.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            Dictionary<string, Action<string>> property_map = new Dictionary<string, Action<string>>
        {
            { "description:", value => drive.Description = value },
            { "product:",     value => drive.Model = drive.Caption = value },
            { "vendor:",      value => drive.Manufacturer = value },
            { "logical name:",value => drive.Name = value },
            { "version:",     value => drive.FirmwareRevision = value },
            { "serial:",      value => drive.SerialNumber = value },
            { "size:",        value => drive.Size = ExtractSizeInBytes(value) }
        };

            foreach (string line in lines)
            {
                string trimmed_line = line.Trim();
                KeyValuePair<string, Action<string>> mapping = property_map.FirstOrDefault(p => trimmed_line.StartsWith(p.Key));
                if (mapping.Key != null)
                {
                    string value = trimmed_line[mapping.Key.Length..].Trim();
                    mapping.Value(value);
                }
            }
            drives.Add(drive);
        }

        return drives;
    }

    private static ulong ExtractSizeInBytes(string input)
    {
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

    private static List<Keyboard> GetKeyboard()
    {
        string read_process_out = ProcessInfo.ReadProcessOut("cat", "/proc/bus/input/devices");
        string[] input_lines = read_process_out.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        Regex regex = new Regex("N: Name=\"([^\"]+)\"", RegexOptions.Compiled);

        return input_lines
        .Where(line => line.Contains("keyboard", StringComparison.OrdinalIgnoreCase) && line.Trim().StartsWith("N: Name="))
        .Select(line => regex.Match(line))
        .Where(match => match.Success)
        .Select(match => match.Groups[1].Value)
        .Select(name => new Keyboard
        {
            Caption = name,
            Description = name,
            Name = name
        })
        .ToList();
    }

    private static List<Memory> GetMemory()
    {
        string process_out = ProcessInfo.ReadProcessOut("system_profiler", "SPMemoryDataType");
        string[] memory_blocks = Regex.Split(process_out, @"(?=Bank\s\d+/)", RegexOptions.Multiline);
        List<Memory> memory_list = new List<Memory>();

        foreach (string block in memory_blocks)
        {
            string trimmed_block = block.Trim();

            if (!trimmed_block.StartsWith("Bank"))
            {
                continue;
            }

            Memory memory = new Memory();
            string[] lines = trimmed_block.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, Action<string>> property_map = new Dictionary<string, Action<string>>
        {
            { "Size:", value => memory.Capacity = ExtractSizeInBytes(value) },
            { "Type:", value =>
                {
                    if (Enum.TryParse(value, out FormFactor tempFormFactor))
                    {
                        memory.FormFactor = tempFormFactor;
                    }
                }
            },
            { "Speed:", value =>
                {
                    if (uint.TryParse(value.Split(' ')[0], out uint temp_speed))
                    {
                        memory.Speed = temp_speed;
                    }
                }
            },
            { "Manufacturer:",  value => memory.Manufacturer = value },
            { "Part Number:",   value => memory.PartNumber = value },
            { "Serial Number:", value => memory.SerialNumber = value }
        };

            foreach (string line in lines)
            {
                string trimmed_line = line.Trim();
                KeyValuePair<string, Action<string>> mapping = property_map.FirstOrDefault(p => trimmed_line.StartsWith(p.Key));

                if (mapping.Key != null)
                {
                    string value = trimmed_line.Substring(mapping.Key.Length).Trim();
                    mapping.Value(value);
                }
            }
            memory_list.Add(memory);
        }

        return memory_list;
    }

    private List<Components.Monitor> GetMonitor()
    {
        string process_out = ProcessInfo.ReadProcessOut("system_profiler", "SPDisplaysDataType");
        string[] device_blocks = Regex.Split(process_out, @"(?=^\s{4}[^\s].*:\s*$)", RegexOptions.Multiline);
        List<Components.Monitor> monitor_list = new List<Components.Monitor>();

        foreach (string block in device_blocks)
        {
            if (!block.Contains("Display Type:"))
            {
                continue;
            }

            Components.Monitor monitor = new Components.Monitor();
            string[] lines = block.Trim().Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            string? name_line = lines.FirstOrDefault();
            if (name_line != null)
            {
                monitor.Name = name_line.Trim().TrimEnd(':');
                monitor.Caption = monitor.Name;
                monitor.Description = monitor.Name;
                monitor.UserFriendlyName = monitor.Name;
            }

            Dictionary<string, Action<string>> property_map = new Dictionary<string, Action<string>>
        {
            { "Display Type:", value => monitor.MonitorType = value },
            { "Online:", value => monitor.Active = value.Trim().Equals("Yes", StringComparison.OrdinalIgnoreCase) },
            { "Display Serial Number:", value => monitor.SerialNumberID = value },
            { "Resolution:", value =>
                {
                    string resolution_part = value.Split(' ')[0];
                    string[] dimensions = resolution_part.Split('x');

                    if (dimensions.Length == 2 &&
                        uint.TryParse(dimensions[0].Trim(), out uint width) &&
                        uint.TryParse(dimensions[1].Trim(), out uint height))
                    {
                        monitor.Resolution = new ScreenResolution(width, height);
                    }
                }
            }
        };

            foreach (string line in lines.Skip(1))
            {
                string trimmedLine = line.Trim();
                KeyValuePair<string, Action<string>> mapping = property_map.FirstOrDefault(p => trimmedLine.StartsWith(p.Key));

                if (mapping.Key != null)
                {
                    string value = trimmedLine.Substring(mapping.Key.Length).Trim();
                    mapping.Value(value);
                }
            }
            monitor_list.Add(monitor);
        }

        return monitor_list;
    }
}