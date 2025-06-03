using System.Diagnostics;

namespace MacAgent.Services;

public class ProcessAppService
{
    public static string ReadProcessOut(string cmd, string args)
    {
        try
        {
            using Process? process = StartProcess(cmd, args);
            using StreamReader? streamReader = process?.StandardOutput;
            process?.WaitForExit();

            return streamReader?.ReadToEnd().Trim() ?? string.Empty;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    private static Process? StartProcess(string cmd, string args)
    {
        ProcessStartInfo processStartInfo = new(cmd, args)
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true
        };

        return Process.Start(processStartInfo);
    }
}
