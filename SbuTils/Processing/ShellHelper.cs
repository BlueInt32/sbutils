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
        var processResult = await RunAsync(startInfo);

        runReport.ExecutionRunTime = processResult.RunTime;
        runReport.ExitCode = processResult.ExitCode;

        runReport.StandardOutput = processResult.StandardOutput;
        runReport.ErrorsOutput = processResult.StandardError;

        return runReport;
    }

    /// <summary>
    /// Runs asynchronous process.
    /// </summary>
    /// <param name="fileName">An application or document which starts the process.</param>
    public static Task<ProcessResults> RunAsync(string fileName) =>
        RunAsync(new ProcessStartInfo(fileName));

    /// <summary>
    /// Runs asynchronous process.
    /// </summary>
    /// <param name="fileName">An application or document which starts the process.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    public static Task<ProcessResults> RunAsync(
        string fileName,
        CancellationToken cancellationToken
    ) => RunAsync(new ProcessStartInfo(fileName), cancellationToken);

    /// <summary>
    /// Runs asynchronous process.
    /// </summary>
    /// <param name="fileName">An application or document which starts the process.</param>
    /// <param name="arguments">Command-line arguments to pass to the application when the process starts.</param>
    public static Task<ProcessResults> RunAsync(string fileName, string arguments) =>
        RunAsync(new ProcessStartInfo(fileName, arguments));

    /// <summary>
    /// Runs asynchronous process.
    /// </summary>
    /// <param name="fileName">An application or document which starts the process.</param>
    /// <param name="arguments">Command-line arguments to pass to the application when the process starts.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    public static Task<ProcessResults> RunAsync(
        string fileName,
        string arguments,
        CancellationToken cancellationToken
    ) => RunAsync(new ProcessStartInfo(fileName, arguments), cancellationToken);

    /// <summary>
    /// Runs asynchronous process.
    /// </summary>
    /// <param name="processStartInfo">The <see cref="T:System.Diagnostics.ProcessStartInfo" /> that contains the information that is used to start the process, including the file name and any command-line arguments.</param>
    public static Task<ProcessResults> RunAsync(ProcessStartInfo processStartInfo) =>
        RunAsync(processStartInfo, CancellationToken.None);

    /// <summary>
    /// Runs asynchronous process.
    /// </summary>
    /// <param name="processStartInfo">The <see cref="T:System.Diagnostics.ProcessStartInfo" /> that contains the information that is used to start the process, including the file name and any command-line arguments.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    public static Task<ProcessResults> RunAsync(
        ProcessStartInfo processStartInfo,
        CancellationToken cancellationToken
    ) =>
        RunAsync(
            processStartInfo,
            new List<string>(),
            new List<string>(),
            cancellationToken,
            CaptureOutput,
            CaptureError
        );

    public static async Task<ProcessResults> RunAsync(
        ProcessStartInfo processStartInfo,
        List<string> standardOutput,
        List<string> standardError,
        CancellationToken cancellationToken,
        Action<object, DataReceivedEventArgs> outputDataReceived,
        Action<object, DataReceivedEventArgs> errorDataReceived
    )
    {
        // force some settings in the start info so we can capture the output
        processStartInfo.UseShellExecute = false;
        processStartInfo.RedirectStandardOutput = true;
        processStartInfo.RedirectStandardError = true;

        var tcs = new TaskCompletionSource<ProcessResults>();

        var process = new Process { StartInfo = processStartInfo, EnableRaisingEvents = true };

        var standardOutputResults = new TaskCompletionSource<string[]>();
        process.OutputDataReceived += CaptureOutput;
        process.OutputDataReceived += (sender, args) =>
        {
            if (args.Data != null)
            {
                standardOutput.Add(args.Data);
            }
            else
            {
                standardOutputResults.SetResult(standardOutput.ToArray());
            }
        };

        var standardErrorResults = new TaskCompletionSource<string[]>();
        process.ErrorDataReceived += CaptureError;
        process.ErrorDataReceived += (sender, args) =>
        {
            if (args.Data != null)
                standardError.Add(args.Data);
            else
                standardErrorResults.SetResult(standardError.ToArray());
        };

        var processStartTime = new TaskCompletionSource<DateTime>();

        process.Exited += async (sender, args) =>
        {
            // Since the Exited event can happen asynchronously to the output and error events,
            // we await the task results for stdout/stderr to ensure they both closed.  We must await
            // the stdout/stderr tasks instead of just accessing the Result property due to behavior on MacOS.
            // For more details, see the PR at https://github.com/jamesmanning/RunProcessAsTask/pull/16/
            tcs.TrySetResult(
                new ProcessResults(
                    process,
                    await processStartTime.Task.ConfigureAwait(false),
                    await standardOutputResults.Task.ConfigureAwait(false),
                    await standardErrorResults.Task.ConfigureAwait(false)
                )
            );
        };

        using (
            cancellationToken.Register(() =>
            {
                tcs.TrySetCanceled();
                try
                {
                    if (!process.HasExited)
                        process.Kill();
                }
                catch (InvalidOperationException) { }
            })
        )
        {
            cancellationToken.ThrowIfCancellationRequested();

            var startTime = DateTime.Now;
            if (process.Start() == false)
            {
                tcs.TrySetException(new InvalidOperationException("Failed to start process"));
            }
            else
            {
                try
                {
                    startTime = process.StartTime;
                }
                catch (Exception)
                {
                    // best effort to try and get a more accurate start time, but if we fail to access StartTime
                    // (for instance, process has already existed), we still have a valid value to use.
                }
                processStartTime.SetResult(startTime);

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }

            return await tcs.Task.ConfigureAwait(false);
        }
    }

    static void CaptureOutput(object sender, DataReceivedEventArgs e)
    {
        ShowOutput(e.Data, ConsoleColor.Green);
    }

    static void CaptureError(object sender, DataReceivedEventArgs e)
    {
        ShowOutput(e.Data, ConsoleColor.Red);
    }

    static void ShowOutput(string data, ConsoleColor color)
    {
        if (data != null)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine("Received: {0}", data);
            Console.ForegroundColor = oldColor;
        }
    }
}
