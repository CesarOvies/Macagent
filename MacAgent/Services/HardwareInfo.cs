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
        string processOutput = ProcessInfo.ReadProcessOut("pmset", "-g batt");

        Match chargeMatch = Regex.Match(processOutput, @"(\d+)%");
        if (chargeMatch.Success && ushort.TryParse(chargeMatch.Groups[1].Value, out ushort estimatedChargeRemaining))
        {
            battery.EstimatedChargeRemaining = estimatedChargeRemaining;
        }

        Match timeMatch = Regex.Match(processOutput, @"(\d+):(\d+)(?=\s)");
        if (timeMatch.Success && uint.TryParse(timeMatch.Groups[1].Value, out uint hours) && uint.TryParse(timeMatch.Groups[2].Value, out uint minutes))
        {
            battery.EstimatedRunTime = (hours * 60) + minutes;
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
}