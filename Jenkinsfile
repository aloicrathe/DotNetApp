pipeline {
    agent any
    options {
        skipDefaultCheckout()
    }
    triggers {
        pollSCM('H/5 * * * *')
    }
    stages {
        stage('Preparation') {
            steps {
                cleanWs()
                checkout scm
            }
        }
        stage('Build') {
            steps {
                sh '''
                    if [ ! -f ~/.docker/cli-plugins/docker-compose ]; then
                      mkdir -p ~/.docker/cli-plugins
                      curl -SL https://github.com/docker/compose/releases/latest/download/docker-compose-linux-x86_64 \
                        -o ~/.docker/cli-plugins/docker-compose
                      chmod +x ~/.docker/cli-plugins/docker-compose
                    fi

                     docker compose up -d --build
                '''
            }
        }
        stage('Test') {
            steps {
                sh '''
                    docker exec -i todoappdb mariadb -uroot -psekrit << '_EOF_'
                    CREATE DATABASE IF NOT EXISTS todo_test_db;
                    GRANT ALL PRIVILEGES ON todo_test_db.* TO 'todo_usr'@'%';
                    FLUSH PRIVILEGES;
_EOF_

                    docker exec -i todoappdb mariadb -utodo_usr -pletmeinplz todo_test_db << '_EOF_'
                    CREATE TABLE IF NOT EXISTS todos (
                        id INT AUTO_INCREMENT PRIMARY KEY,
                        title VARCHAR(255) NOT NULL,
                        is_done BOOLEAN NOT NULL DEFAULT FALSE,
                        created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
                    );
_EOF_

                    docker run --rm \
                      --volumes-from jenkins_server \
                      -w "$WORKSPACE" \
                      --network container:todoappdb \
                      -u "$(id -u):$(id -g)" -e HOME=/tmp -e DOTNET_CLI_HOME=/tmp \
                      mcr.microsoft.com/dotnet/sdk:10.0 \
                      dotnet test
                '''
            }
        }
    }
}
