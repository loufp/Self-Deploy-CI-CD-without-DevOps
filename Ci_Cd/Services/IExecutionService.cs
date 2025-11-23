namespace Ci_Cd.Services
{
    public interface IExecutionService
    {
        Task<ExecutionResult> Run(string workingDirectory, IEnumerable<string> commands, TimeSpan timeout);
        Task<ExecutionResult> RunInContainer(string workingDirectory, IEnumerable<string> commands, string image, TimeSpan timeout);
    }

    public class ExecutionResult
    {
        public int ExitCode { get; set; }
        public string StdOut { get; set; } = string.Empty;
        public string StdErr { get; set; } = string.Empty;
    }
}
