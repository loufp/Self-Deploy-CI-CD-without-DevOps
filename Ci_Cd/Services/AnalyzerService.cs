using System.IO;
using System.Linq;
using System.Text.Json;
using Ci_Cd.Models;

namespace Ci_Cd.Services
{
    public class AnalyzerService : IAnalyzerService
    {
        public RepoAnalysisResult Analyze(string repoPath)
        {
            var result = new RepoAnalysisResult();

            // 1. Проверяем Docker (включая вложенные папки и Dockerfile*)
            var dockerFiles = Directory.GetFiles(repoPath, "Dockerfile*", SearchOption.AllDirectories);
            if (dockerFiles.Any())
            {
                result.HasDockerfile = true;
            }

            // 2. Определяем стек технологий (рекурсивно)
            if (Directory.GetFiles(repoPath, "*.csproj", SearchOption.AllDirectories).Any())
            {
                result.Language = RepoAnalysisResult.ProjectLanguage.DotNet;
                result.Framework = "DotNet Core";
                result.SuggestedBuildCommands.Add("dotnet restore");
                result.SuggestedBuildCommands.Add("dotnet build");
                result.SuggestedBuildCommands.Add("dotnet test");
            }
            else if (Directory.GetFiles(repoPath, "package.json", SearchOption.AllDirectories).Any())
            {
                result.Language = RepoAnalysisResult.ProjectLanguage.NodeJs;
                result.Framework = "Node.js Generic";
                // read package.json to determine test script and install tool
                var pkgPath = Directory.GetFiles(repoPath, "package.json", SearchOption.AllDirectories).First();
                try
                {
                    var json = File.ReadAllText(pkgPath);
                    var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("scripts", out var scripts) && scripts.ValueKind == JsonValueKind.Object)
                    {
                        if (scripts.TryGetProperty("test", out var t) && t.ValueKind == JsonValueKind.String)
                        {
                            result.SuggestedBuildCommands.Add("npm install");
                            result.SuggestedBuildCommands.Add("npm test");
                        }
                        else
                        {
                            result.SuggestedBuildCommands.Add("npm install");
                        }
                    }
                    else
                    {
                        result.SuggestedBuildCommands.Add("npm install");
                    }
                }
                catch
                {
                    result.SuggestedBuildCommands.Add("npm install");
                }
            }
            else if (Directory.GetFiles(repoPath, "go.mod", SearchOption.AllDirectories).Any())
            {
                result.Language = RepoAnalysisResult.ProjectLanguage.Go;
                result.Framework = "Go Modules";
                result.SuggestedBuildCommands.Add("go build");
                result.SuggestedBuildCommands.Add("go test ./...");
            }
            else if (Directory.GetFiles(repoPath, "requirements.txt", SearchOption.AllDirectories).Any() ||
                     Directory.GetFiles(repoPath, "setup.py", SearchOption.AllDirectories).Any() ||
                     Directory.GetFiles(repoPath, "pyproject.toml", SearchOption.AllDirectories).Any())
            {
                result.Language = RepoAnalysisResult.ProjectLanguage.Python;
                result.Framework = "Python";
                result.SuggestedBuildCommands.Add("pip install -r requirements.txt");
                result.SuggestedBuildCommands.Add("pytest");
            }
            else if (Directory.GetFiles(repoPath, "pom.xml", SearchOption.AllDirectories).Any())
            {
                result.Language = RepoAnalysisResult.ProjectLanguage.Java;
                result.Framework = "Maven";
                result.SuggestedBuildCommands.Add("mvn clean install");
                result.SuggestedBuildCommands.Add("mvn test");
            }
            else if (Directory.GetFiles(repoPath, "build.gradle", SearchOption.AllDirectories).Any() ||
                     Directory.GetFiles(repoPath, "build.gradle.kts", SearchOption.AllDirectories).Any())
            {
                result.Language = RepoAnalysisResult.ProjectLanguage.Java;
                result.Framework = "Gradle";
                result.SuggestedBuildCommands.Add("gradle build");
                result.SuggestedBuildCommands.Add("gradle test");
            }

            return result;
        }
    }
}