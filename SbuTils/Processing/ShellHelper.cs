using System.Diagnostics;
using RunProcessAsTask;

namespace SbuTils.Processing;

public static class ShellHelper
{
    public static async Task<RunReport> RunCommand(string executable, string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = executable,
            Arguments = arguments,
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardInput = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        var runReport = new RunReport();
        runReport.InputProgram = executable;
        runReport.InputArgs = arguments;
        var processResult = await ProcessEx.RunAsync(startInfo);
        runReport.ExitCode = processResult.ExitCode;
        runReport.ExecutionRunTime = processResult.RunTime;

        runReport.StandardOutput = processResult.StandardOutput;
        runReport.ErrorsOutput = processResult.StandardError;

        return runReport;
    }
}
