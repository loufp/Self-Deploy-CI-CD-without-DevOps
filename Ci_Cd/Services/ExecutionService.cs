using System.Diagnostics;

namespace Ci_Cd.Services
{
    public class ExecutionService : IExecutionService
    {
        public async Task<ExecutionResult> Run(string workingDirectory, IEnumerable<string> commands, TimeSpan timeout)
        {
            var result = new ExecutionResult();
            var sbOut = new System.Text.StringBuilder();
            var sbErr = new System.Text.StringBuilder();

            foreach (var cmd in commands)
            {
                // split into program and args (simple)
                var parts = SplitCommand(cmd);
                if (parts.Length == 0) continue;
                var psi = new ProcessStartInfo(parts[0], string.Join(' ', parts.Skip(1)))
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    WorkingDirectory = workingDirectory,
                    CreateNoWindow = true
                };

                using var p = new Process { StartInfo = psi };
                p.OutputDataReceived += (s, e) => { if (e.Data != null) sbOut.AppendLine(e.Data); };
                p.ErrorDataReceived += (s, e) => { if (e.Data != null) sbErr.AppendLine(e.Data); };

                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();

                var exited = await Task.Run(() => p.WaitForExit((int)timeout.TotalMilliseconds));
                if (!exited)
                {
                    try { p.Kill(); } catch { }
                    sbErr.AppendLine($"Command timed out: {cmd}");
                    result.ExitCode = -1;
                    result.StdOut = sbOut.ToString();
                    result.StdErr = sbErr.ToString();
                    return result;
                }

                result.ExitCode = p.ExitCode;
                if (p.ExitCode != 0)
                {
                    // stop on first failure
                    result.StdOut = sbOut.ToString();
                    result.StdErr = sbErr.ToString();
                    return result;
                }
            }

            result.StdOut = sbOut.ToString();
            result.StdErr = sbErr.ToString();
            return result;
        }

        public async Task<ExecutionResult> RunInContainer(string workingDirectory, IEnumerable<string> commands, string image, TimeSpan timeout)
        {
            // Build a single shell command that will run inside container
            var script = string.Join(" && ", commands.Select(c => c.Replace("\"", "\\\"")));
            var containerCmd = $"/bin/sh -c \"{script}\"";

            var result = new ExecutionResult();
            var sbOut = new System.Text.StringBuilder();
            var sbErr = new System.Text.StringBuilder();

            // Full docker run command
            var args = $"run --rm -v \"{workingDirectory}:/work\" -w /work {image} /bin/sh -c \"{script}\"";

            // Проверка наличия docker
            try
            {
                var check = new ProcessStartInfo("docker", "version --format '{{.Client.Version}}'")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var pc = Process.Start(check);
                if (pc == null)
                {
                    result.ExitCode = -2;
                    result.StdErr = "Docker not available (Process start failed)";
                    return result;
                }
                pc.WaitForExit(2000);
                if (pc.ExitCode != 0)
                {
                    result.ExitCode = -2;
                    result.StdErr = pc.StandardError.ReadToEnd();
                    return result;
                }
            }
            catch (Exception exCheck)
            {
                result.ExitCode = -2;
                result.StdErr = exCheck.Message;
                return result;
            }

            var psi = new ProcessStartInfo("docker", args)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var p = new Process { StartInfo = psi };
            p.OutputDataReceived += (s, e) => { if (e.Data != null) sbOut.AppendLine(e.Data); };
            p.ErrorDataReceived += (s, e) => { if (e.Data != null) sbErr.AppendLine(e.Data); };

            try
            {
                p.Start();
            }
            catch (Exception ex)
            {
                result.ExitCode = -1;
                result.StdErr = ex.Message;
                return result;
            }

            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            var exited = await Task.Run(() => p.WaitForExit((int)timeout.TotalMilliseconds));
            if (!exited)
            {
                try { p.Kill(); } catch { }
                sbErr.AppendLine("Container command timed out");
                result.ExitCode = -1;
                result.StdOut = sbOut.ToString();
                result.StdErr = sbErr.ToString();
                return result;
            }

            result.ExitCode = p.ExitCode;
            result.StdOut = sbOut.ToString();
            result.StdErr = sbErr.ToString();
            return result;
        }

        private static string[] SplitCommand(string cmd)
        {
            // очень простой парсер — можно заменить на более надёжный
            return cmd.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
