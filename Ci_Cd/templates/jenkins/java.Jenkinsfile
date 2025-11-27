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
        
        stage('Build') {
            steps {
                echo 'Building Maven project...'
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
                    junit testResults: '**/target/surefire-reports/*.xml', allowEmptyResults: true
                }
            }
        }
        
        stage('SonarQube Analysis') {
            when { expression { env.SONAR_TOKEN != null } }
            steps {
                withSonarQubeEnv('SonarQube') {
                    sh '''
                        sonar-scanner \
                          -Dsonar.projectKey=${PROJECT_NAME} \
                          -Dsonar.sources=src \
                          -Dsonar.java.binaries=target/classes
                    '''
                }
            }
        }
        
        stage('Package') {
            steps {
                echo 'Creating Docker image...'
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
                echo 'Deploying to staging environment...'
                sh 'echo "Deploy to staging: docker run -d ${DOCKER_IMAGE_NAME}"'
            }
        }
    }
    
    post {
        always {
            cleanWs()
        }
        success {
            echo 'Pipeline completed successfully!'
        }
        failure {
            echo 'Pipeline failed!'
        }
    }
}

