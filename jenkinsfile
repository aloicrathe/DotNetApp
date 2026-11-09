items:
- freestyle:
    buildWrappers:
    - preBuildCleanup:
        deleteDirs: false
        disableDeferredWipeout: false
    builders:
    - shell:
        command: |2-

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
    concurrentBuild: false
    displayName: "TestDotNet-Demo-App"
    name: "TestDotNet-Demo-App"
    quietPeriod: 5
    scm:
      scmGit:
        branches:
        - name: "*/main"
        buildChooser: "default"
        userRemoteConfigs:
        - url: "https://github.com/aloicrathe/DotNetApp"
    scmCheckoutStrategy: "standard"
