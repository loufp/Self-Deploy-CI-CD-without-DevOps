using Ci_Cd.Models;

namespace Ci_Cd.Services;

public interface ITemplateService
{
    string GenerateGitLabCi(RepoAnalysisResult analysis);
    string GenerateJenkinsfile(RepoAnalysisResult analysis);
}

