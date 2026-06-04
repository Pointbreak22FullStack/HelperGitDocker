using System.Diagnostics;

namespace HelperGitDocker;

internal static class CommandRunner
{
    public static int Run(string fileName, string arguments, string? workingDirectory = null)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            if (!string.IsNullOrWhiteSpace(workingDirectory))
            {
                startInfo.WorkingDirectory = workingDirectory;
            }

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                Console.WriteLine($"Failed to start command: {fileName} {arguments}");
                return -1;
            }

            process.OutputDataReceived += (sender, eventArgs) => { if (eventArgs.Data != null) Console.WriteLine(eventArgs.Data); };
            process.ErrorDataReceived += (sender, eventArgs) => { if (eventArgs.Data != null) Console.Error.WriteLine(eventArgs.Data); };
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            return process.ExitCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Command execution failed: {ex.Message}");
            return -1;
        }
    }
}
