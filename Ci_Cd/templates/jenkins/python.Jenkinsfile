pipeline {
    agent { docker { image '{{IMAGE}}' } }
    
    environment {
        PROJECT_NAME = '{{PROJECT_NAME}}'
        DOCKER_IMAGE_NAME = "${PROJECT_NAME}:${BUILD_NUMBER}"
    }
    
    stages {
        stage('Checkout') {
            steps {
                checkout scm
                echo 'Code checked out successfully'
            }
        }
        
        stage('Install Dependencies') {
            steps {
                echo 'Installing Python dependencies...'
                sh '{{BUILD_CMDS_JENKINS}}'
            }
        }
        
        stage('Test') {
            steps {
                echo 'Running Python tests...'
                sh '{{TEST_CMDS_JENKINS}}'
            }
            post {
                always {
                    publishHTML([
                        allowMissing: false,
                        alwaysLinkToLastBuild: true,
                        keepAll: true,
                        reportDir: 'htmlcov',
                        reportFiles: 'index.html',
                        reportName: 'Coverage Report'
                    ])
                }
            }
        }
        
        stage('Code Quality') {
            steps {
                echo 'Running code quality checks...'
                sh 'flake8 . --max-line-length=88 --extend-ignore=E203,W503 || true'
                sh 'black --check . || true'
            }
        }
        
        stage('Package') {
            steps {
                echo 'Building Docker image...'
                sh "docker build -t ${DOCKER_IMAGE_NAME} ."
                sh "docker tag ${DOCKER_IMAGE_NAME} ${PROJECT_NAME}:latest"
            }
        }
        
        stage('Publish') {
            when { branch 'main' }
            steps {
                echo 'Publishing artifacts...'
                sh "docker push ${DOCKER_IMAGE_NAME}"
                sh "docker push ${PROJECT_NAME}:latest"
            }
        }
        
        stage('Deploy Staging') {
            when { branch 'main' }
            steps {
                echo 'Deploying to staging...'
                sh 'echo "Deploy: docker run -d -p 8000:8000 ${DOCKER_IMAGE_NAME}"'
            }
        }
    }
    
    post {
        always {
            cleanWs()
        }
    }
}

