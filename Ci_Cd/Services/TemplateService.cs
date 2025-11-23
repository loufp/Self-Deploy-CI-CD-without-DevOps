using System.Text;
using Ci_Cd.Models;

namespace Ci_Cd.Services
{

    public class TemplateService : ITemplateService
    {
        public string GenerateGitLabCi(RepoAnalysisResult analysis)
        {
            var sb = new StringBuilder();

            string image = analysis.Language switch
            {
                RepoAnalysisResult.ProjectLanguage.DotNet => "mcr.microsoft.com/dotnet/sdk:8.0",
                RepoAnalysisResult.ProjectLanguage.NodeJs => "node:18-alpine",
                RepoAnalysisResult.ProjectLanguage.Go => "golang:1.21",
                RepoAnalysisResult.ProjectLanguage.Python => "python:3.10",
                _ => "ubuntu:latest"
            };

            sb.AppendLine($"image: {image}");
            sb.AppendLine("");

            //Определение стадий 
            sb.AppendLine("stages:");
            sb.AppendLine("  - build");
            sb.AppendLine("  - test");
            // если есть Dockerfile, добавляем стадии package и deploy
            if (analysis.HasDockerfile)
            {
                sb.AppendLine("  - package");
                sb.AppendLine("  - deploy");
            }
            sb.AppendLine("");

            //Генерация скриптов 
            sb.AppendLine("build_job:");
            sb.AppendLine("  stage: build");
            sb.AppendLine("  script:");
            
            if (analysis.SuggestedBuildCommands.Any())
            {
                foreach (var cmd in analysis.SuggestedBuildCommands)
                {
                    sb.AppendLine($"    - {cmd}");
                }
            }
            else
            {
                sb.AppendLine("    - echo 'No build commands detected'");
            }
            sb.AppendLine("");

            // Пример работы с артефактами (если нужно)
            sb.AppendLine("test_job:");
            sb.AppendLine("  stage: test");
            sb.AppendLine("  script:");
            sb.AppendLine("    - echo 'Running tests...'");
            // Добавляем команды тестирования в зависимости от языка
            if (analysis.Language == RepoAnalysisResult.ProjectLanguage.DotNet)
            {
                sb.AppendLine("    - dotnet test");
            }
            else if (analysis.Language == RepoAnalysisResult.ProjectLanguage.NodeJs)
            {
                sb.AppendLine("    - npm test");
            }
            else if (analysis.Language == RepoAnalysisResult.ProjectLanguage.Go)
            {
                sb.AppendLine("    - go test ./...");
            }
            else if (analysis.Language == RepoAnalysisResult.ProjectLanguage.Python)
            {
                sb.AppendLine("    - pytest");
            }
            sb.AppendLine("");

            if (analysis.HasDockerfile)
            {
                sb.AppendLine("package_job:");
                sb.AppendLine("  stage: package");
                sb.AppendLine("  script:");
                sb.AppendLine("    - docker build -t myapp:latest .");
                sb.AppendLine("    - echo 'Docker image built successfully'");
                sb.AppendLine("");
                
                sb.AppendLine("deploy_job:");
                sb.AppendLine("  stage: deploy");
                sb.AppendLine("  script:");
                sb.AppendLine("    - echo 'Deploy: tagging and pushing image to registry (requires CI_REGISTRY and credentials)'");
                sb.AppendLine("    - if [ -n \"$CI_REGISTRY\" ] && [ -n \"$CI_REGISTRY_IMAGE\" ]; then docker tag myapp:latest $CI_REGISTRY_IMAGE:latest && docker push $CI_REGISTRY_IMAGE:latest; else echo 'CI registry variables not set, skipping push'; fi");
                sb.AppendLine("    - if [ -n \"$KUBE_CONFIG_BASE64\" ]; then echo $KUBE_CONFIG_BASE64 | base64 -d > kubeconfig && KUBECONFIG=kubeconfig kubectl apply -f k8s/ || echo 'no k8s manifests or KUBE_CONFIG_BASE64 not set'; fi");
                sb.AppendLine("");
            }
            
            return sb.ToString();
        }

        public string GenerateJenkinsfile(RepoAnalysisResult analysis)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("pipeline {");
            
            string? dockerImage = analysis.Language switch
            {
                RepoAnalysisResult.ProjectLanguage.DotNet => "mcr.microsoft.com/dotnet/sdk:8.0",
                RepoAnalysisResult.ProjectLanguage.NodeJs => "node:18-alpine",
                RepoAnalysisResult.ProjectLanguage.Go => "golang:1.21",
                RepoAnalysisResult.ProjectLanguage.Python => "python:3.10",
                _ => null
            };

            if (!string.IsNullOrEmpty(dockerImage))
            {
                sb.AppendLine($"    agent {{");
                sb.AppendLine($"        docker {{ image '{dockerImage}' }}");
                sb.AppendLine("    }");
            }
            else
            {
                sb.AppendLine("    agent any");
            }
            
            sb.AppendLine("");
            sb.AppendLine("    stages {");
            
            sb.AppendLine("        stage('Build') {");
            sb.AppendLine("            steps {");
            if (analysis.SuggestedBuildCommands.Any())
            {
                foreach (var cmd in analysis.SuggestedBuildCommands)
                {
                    sb.AppendLine($"                sh '{cmd}'");
                }
            }
            else
            {
                sb.AppendLine("                echo 'No build commands detected'");
            }
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine("");
            
            sb.AppendLine("        stage('Test') {");
            sb.AppendLine("            steps {");
            if (analysis.Language == RepoAnalysisResult.ProjectLanguage.DotNet)
            {
                sb.AppendLine("                sh 'dotnet test'");
            }
            else if (analysis.Language == RepoAnalysisResult.ProjectLanguage.NodeJs)
            {
                sb.AppendLine("                sh 'npm test'");
            }
            else if (analysis.Language == RepoAnalysisResult.ProjectLanguage.Go)
            {
                sb.AppendLine("                sh 'go test ./...'");
            }
            else if (analysis.Language == RepoAnalysisResult.ProjectLanguage.Python)
            {
                sb.AppendLine("                sh 'pytest'");
            }
            else
            {
                sb.AppendLine("                echo 'Running tests...'");
            }
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine("");
            
            if (analysis.HasDockerfile)
            {
                sb.AppendLine("        stage('Package') {");
                sb.AppendLine("            steps {");
                sb.AppendLine("                sh 'docker build -t myapp:latest .' ");
                sb.AppendLine("                echo 'Docker image built successfully'");
                sb.AppendLine("            }");
                sb.AppendLine("        }");
                sb.AppendLine("");
                sb.AppendLine("        stage('Deploy') {");
                sb.AppendLine("            steps {");
                sb.AppendLine("                sh \"if [ -n '$DOCKER_REGISTRY' ]; then docker tag myapp:latest $DOCKER_REGISTRY/myapp:latest && docker push $DOCKER_REGISTRY/myapp:latest; else echo 'DOCKER_REGISTRY not set, skipping push'; fi\"");
                sb.AppendLine("                sh \"if [ -n '$KUBE_CONFIG_BASE64' ]; then echo $KUBE_CONFIG_BASE64 | base64 -d > kubeconfig && KUBECONFIG=kubeconfig kubectl apply -f k8s/; else echo 'KUBE_CONFIG_BASE64 not set, skipping k8s apply'; fi\"");
                sb.AppendLine("            }");
                sb.AppendLine("        }");
                sb.AppendLine("");
            }
            
            sb.AppendLine("    }");
            sb.AppendLine("}");
            
            return sb.ToString();
        }
    }
}