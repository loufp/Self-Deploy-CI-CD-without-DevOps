pipeline {
    agent { docker { image '{{IMAGE}}' } }
    
    environment {
        PROJECT_NAME = '{{PROJECT_NAME}}'
        DOCKER_IMAGE_NAME = "${PROJECT_NAME}:${BUILD_NUMBER}"
        CI = 'true'
    }
    
    stages {
        stage('Checkout') {
            steps {
                checkout scm
                echo 'Code checked out successfully'
            }
        }
        
        stage('Install') {
            steps {
                echo 'Installing Node.js dependencies...'
                sh 'npm ci'
            }
        }
        
        stage('Build') {
            steps {
                echo 'Building Node.js project...'
                sh '{{BUILD_CMDS_JENKINS}}'
            }
        }
        
        stage('Test') {
            steps {
                echo 'Running tests...'
                sh '{{TEST_CMDS_JENKINS}}'
            }
            post {
                always {
                    publishHTML([
                        allowMissing: false,
                        alwaysLinkToLastBuild: true,
                        keepAll: true,
                        reportDir: 'coverage',
                        reportFiles: 'index.html',
                        reportName: 'Test Coverage'
                    ])
                }
            }
        }
        
        stage('Lint') {
            steps {
                echo 'Running ESLint...'
                sh 'npm run lint || true'
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
                echo 'Publishing to registry...'
                sh "docker push ${DOCKER_IMAGE_NAME}"
                sh "docker push ${PROJECT_NAME}:latest"
            }
        }
        
        stage('Deploy Staging') {
            when { branch 'main' }
            steps {
                echo 'Deploying to staging...'
                sh 'echo "Deploy: docker run -d -p 3000:3000 ${DOCKER_IMAGE_NAME}"'
            }
        }
    }
    
    post {
        always {
            cleanWs()
        }
    }
}

