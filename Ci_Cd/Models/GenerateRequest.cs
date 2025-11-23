namespace Ci_Cd.Models
{
    public class GenerateRequest
    {
        public string RepoUrl { get; set; } = string.Empty;
        //если true—выполнит
        public bool Execute { get; set; } = false;
    }
}

