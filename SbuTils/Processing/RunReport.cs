using System.Text;

namespace SbuTils.Processing;

public class RunReport
{
    public string InputCommand => $"{InputProgram} {InputArgs}";
    public string InputProgram { get; set; } = string.Empty;
    public string InputArgs { get; set; } = string.Empty;
    public int ExitCode { get; set; }
    public TimeSpan ExecutionRunTime { get; set; }
    public string[] StandardOutput { get; set; } = [];
    public string[] ErrorsOutput { get; set; } = [];

    /// <summary>
    /// Execute the provided action for each line in a fancy report. Can be used with your logger.
    /// </summary>
    /// <param name="action"></param>
    public void FancyReportEnumerator(Action<string> action)
    {
        action.Invoke($"Run report for {InputProgram}: ");
        action.Invoke($"  * full command:");
        action.Invoke($"    {InputCommand}");
        action.Invoke($"  * exec time: {ExecutionRunTime.Milliseconds}ms");
        action.Invoke($"  * exit code: {ExitCode}");
        action.Invoke($"  * output ({StandardOutput.Length} lines):");
        foreach (var std in StandardOutput)
        {
            action.Invoke($"    {std}");
        }
        action.Invoke($"  * errors ({ErrorsOutput.Length} lines):");
        foreach (var err in ErrorsOutput)
        {
            action.Invoke($"    {err}");
        }
    }

    /// <summary>
    /// Execute the provided action for each line in a fancy report. Can be used with your logger.
    /// </summary>
    /// <param name="action"></param>
    public string FancyReport
    {
        get
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Run report for {InputProgram}: ");
            sb.AppendLine($"  * full command:");
            sb.AppendLine($"    {InputCommand}");
            sb.AppendLine($"  * exec time: {ExecutionRunTime.Milliseconds}ms");
            sb.AppendLine($"  * exit code: {ExitCode}");
            sb.AppendLine($"  * output ({StandardOutput.Length} lines):");
            foreach (var std in StandardOutput)
            {
                sb.AppendLine($"    {std}");
            }

            sb.AppendLine($"  * errors ({ErrorsOutput.Length} lines):");
            foreach (var err in ErrorsOutput)
            {
                sb.AppendLine($"    {err}");
            }

            return sb.ToString();
        }
    }
}
