pipeline {
    agent { docker { image '{{IMAGE}}' } }
    
    environment {
        PROJECT_NAME = '{{PROJECT_NAME}}'
        DOCKER_IMAGE_NAME = "${PROJECT_NAME}:${BUILD_NUMBER}"
        GOPROXY = 'https://proxy.golang.org,direct'
        GOSUMDB = 'sum.golang.org'
    }
    
    stages {
        stage('Checkout') {
            steps {
                checkout scm
                echo 'Code checked out successfully'
            }
        }
        
        stage('Dependencies') {
            steps {
                echo 'Downloading Go modules...'
                sh 'go mod download'
                sh 'go mod verify'
            }
        }
        
        stage('Build') {
            steps {
                echo 'Building Go application...'
                sh '{{BUILD_CMDS_JENKINS}}'
            }
        }
        
        stage('Test') {
            steps {
                echo 'Running Go tests...'
                sh '{{TEST_CMDS_JENKINS}}'
            }
            post {
                always {
                    publishHTML([
                        allowMissing: false,
                        alwaysLinkToLastBuild: true,
                        keepAll: true,
                        reportDir: '.',
                        reportFiles: 'coverage.html',
                        reportName: 'Go Coverage Report'
                    ])
                }
            }
        }
        
        stage('Quality Checks') {
            steps {
                echo 'Running Go quality checks...'
                sh 'go vet ./... || true'
                sh 'golint ./... || true'
                sh 'go fmt -l . || true'
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
                sh 'echo "Deploy: docker run -d -p 8080:8080 ${DOCKER_IMAGE_NAME}"'
            }
        }
    }
    
    post {
        always {
            cleanWs()
        }
    }
}

