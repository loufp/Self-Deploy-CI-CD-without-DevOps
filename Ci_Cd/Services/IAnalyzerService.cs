using Ci_Cd.Models;

namespace Ci_Cd.Services;

public interface IAnalyzerService
{
    RepoAnalysisResult Analyze(string repoPath);
}