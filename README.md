# Self-Deploy: CI/CD Generator

🚀 **Автоматическая генерация CI/CD пайплайнов по анализу структуры Git-репозитория**

## 📋 Описание

Этот проект автоматизирует создание CI/CD конфигураций для различных проектов. Система клонирует репозиторий, анализирует его структуру, определяет стек технологий и генерирует готовые YAML-шаблоны для **GitLab CI** и **Jenkins**.

## ✨ Возможности

- ✅ **Автоматическое клонирование Git-репозиториев**
- ✅ **Определение языка программирования и фреймворка**:
  - .NET (ASP.NET Core, C#)
  - Node.js / JavaScript
  - Python
  - Go
  - Java (Maven, Gradle)
- ✅ **Анализ конфигурационных файлов**:
  - `*.csproj` → .NET
  - `package.json` → Node.js
  - `requirements.txt`, `setup.py`, `pyproject.toml` → Python
  - `go.mod` → Go
  - `pom.xml` → Maven
  - `build.gradle` → Gradle
  - `Dockerfile` → Docker support
- ✅ **Генерация pipeline для**:
  - GitLab CI (`.gitlab-ci.yml`)
  - Jenkins (`Jenkinsfile`)
- ✅ **Поддержка стадий**:
  - Build (сборка)
  - Test (тестирование)
  - Package (Docker-образы, если есть Dockerfile)

## 🛠 Технологии

- **Backend**: ASP.NET Core 8.0 (C#)
- **Git**: Системный git для клонирования репозиториев
- **API**: RESTful API с Swagger документацией
- **CORS**: Настроен для работы с React/Vue/Angular фронтендами

## 📦 Установка и запуск

### Требования

- .NET 8.0 SDK
- Git (установленный в системе)

### Запуск

```bash
cd Ci_Cd
dotnet restore
dotnet build
dotnet run
```

Приложение запустится на `http://localhost:5034` (или порт из `launchSettings.json`)

### Swagger UI

После запуска откройте в браузере:
```
http://localhost:5034/swagger
```

## 🔌 API

### POST `/api/pipeline/generate`

Генерирует CI/CD pipeline для указанного репозитория.

**Request:**
```json
POST /api/pipeline/generate
Content-Type: application/json

"https://github.com/username/repository.git"
```

**Response:**
```json
{
  "analysis": {
    "language": "DotNet",
    "framework": "DotNet Core",
    "hasDockerfile": true,
    "buildCommands": [
      "dotnet restore",
      "dotnet build",
      "dotnet test"
    ]
  },
  "gitLabCI": "image: mcr.microsoft.com/dotnet/sdk:8.0\n\nstages:\n  - build\n...",
  "jenkinsfile": "pipeline {\n    agent {\n        docker { image 'mcr.microsoft.com/dotnet/sdk:8.0' }\n..."
}
```

## 🎯 Пример использования (React)

```javascript
const generatePipeline = async (repoUrl) => {
  const response = await fetch('http://localhost:5034/api/pipeline/generate', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(repoUrl)
  });

  const data = await response.json();
  
  console.log('Язык:', data.analysis.language);
  console.log('Фреймворк:', data.analysis.framework);
  console.log('GitLab CI:', data.gitLabCI);
  console.log('Jenkinsfile:', data.jenkinsfile);
};

// Использование
generatePipeline('https://github.com/dotnet/aspnetcore');
```

## 🏗 Архитектура проекта

```
Ci_Cd/
├── Controllers/
│   └── PipelineController.cs    # API контроллер
├── Services/
│   ├── IGitServices.cs          # Интерфейс для работы с Git
│   ├── GitServices.cs           # Клонирование репозиториев
│   ├── IAnalyzerService.cs      # Интерфейс анализатора
│   ├── AnalyzerService.cs       # Анализ структуры проекта
│   ├── ITemplateService.cs      # Интерфейс генератора
│   └── TemplateService.cs       # Генерация YAML-шаблонов
├── Models/
│   └── RepoAnalysisResult.cs    # Модель результата анализа
├── wwwroot/                     # Статические файлы (для фронтенда)
└── Program.cs                   # Точка входа приложения
```

## 🔍 Как это работает

1. **Клонирование**: Система создаёт временную папку и клонирует репозиторий с `--depth 1` (только последний коммит)
2. **Анализ**: Сканирует файлы проекта и определяет язык/фреймворк по наличию характерных файлов
3. **Генерация**: На основе анализа создаёт YAML-конфигурации с правильными командами сборки/тестирования
4. **Очистка**: Удаляет временные файлы после генерации

## 📝 Примеры сгенерированных pipeline

### GitLab CI (.NET)

```yaml
image: mcr.microsoft.com/dotnet/sdk:8.0

stages:
  - build
  - test
  - package

build_job:
  stage: build
  script:
    - dotnet restore
    - dotnet build
    - dotnet test

test_job:
  stage: test
  script:
    - echo 'Running tests...'
    - dotnet test

package_job:
  stage: package
  script:
    - docker build -t myapp:latest .
    - echo 'Docker image built successfully'
```

### Jenkinsfile (.NET)

```groovy
pipeline {
    agent {
        docker { image 'mcr.microsoft.com/dotnet/sdk:8.0' }
    }

    stages {
        stage('Build') {
            steps {
                sh 'dotnet restore'
                sh 'dotnet build'
                sh 'dotnet test'
            }
        }

        stage('Test') {
            steps {
                sh 'dotnet test'
            }
        }

        stage('Package') {
            steps {
                sh 'docker build -t myapp:latest .'
                echo 'Docker image built successfully'
            }
        }
    }
}
```

## 🌟 Поддерживаемые стеки

| Язык/Платформа | Определяется по | Команды сборки |
|----------------|-----------------|----------------|
| **.NET** | `*.csproj` | `dotnet restore`, `dotnet build`, `dotnet test` |
| **Node.js** | `package.json` | `npm install`, `npm test` |
| **Python** | `requirements.txt`, `setup.py`, `pyproject.toml` | `pip install -r requirements.txt`, `pytest` |
| **Go** | `go.mod` | `go build`, `go test ./...` |
| **Java (Maven)** | `pom.xml` | `mvn clean install`, `mvn test` |
| **Java (Gradle)** | `build.gradle` | `gradle build`, `gradle test` |

## 🚀 Дальнейшее развитие

- [ ] Поддержка GitHub Actions
- [ ] Определение версий зависимостей
- [ ] Анализ тестов (покрытие кода)
- [ ] Интеграция с облачными платформами (AWS, Azure, GCP)
- [ ] Кеширование зависимостей в pipeline
- [ ] Автоматическое определение переменных окружения
- [ ] Поддержка монорепозиториев

## 📄 Лицензия

MIT

## 👨‍💻 Автор

Проект создан для автоматизации DevOps процессов без необходимости ручной настройки CI/CD.

