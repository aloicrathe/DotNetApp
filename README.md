# dotnet-demo

Minimal demo of a single page webapp that shows the results of a database query.

Vibe coded with Claude Sonnet 5.

Remark that this app was built on a Fedora 44 system with .Net 10.0 SDK and Podman installed. It should work on any system that supports .Net 10.0 and has a container runtime installed. The `podman` command can be replaced with `docker` on a system that has Docker installed.

## Running the app

This section shows two ways to run the app. The first is with podman compose, creating a container for the database and one for the webapp. The other is to build and run the app "natively" on the host system, with a MariaDB database running in a container.

You should be able to run the app with podman (or docker) compose. From the root of the repo, run:

```console
podman compose up -d --build
```

This will start a MariaDB database and the webapp. The webapp will be available at <http://localhost:8080>.

To run the app "natively" (without podman compose), you will need to have the .Net 10.0 SDK installed and a MariaDB database running that has the necessary schema. You will also need to set the `ConnectionStrings__TodoDb` environment variable to point to it (see appsettings.Development.json for an example).

An example for starting a suitable containerized MariaDB database is:

```console
podman pull mariadb:11
podman volume create mariadb-data
podman run -d --name todoappdb -p 3306:3306 -e MARIADB_ROOT_PASSWORD=sekrit -e MARIADB_DATABASE=todo_db -e MARIADB_USER=todo_usr -e MARIADB_PASSWORD=letmeinplz -v mariadb-data:/var/lib/mysql:Z mariadb:11
```

Initialize the database with the schema.sql file:

```console
mariadb -h localhost --port=3306 -utodo_usr -pletmeinplz todo_db < schema.sql
```

You can then run the app with:

```console
cd TodoApp
dotnet run
```

## Running unit tests

The example unit tests are in the `TodoApp.Tests` project. You can run them from the project root dwith:

```console
$ dotnet test
Restore complete (0.4s)
  TodoApp net10.0 succeeded (0.2s) → TodoApp/bin/Debug/net10.0/TodoApp.dll
  TodoApp.Tests net10.0 succeeded (0.2s) → TodoApp.Tests/bin/Debug/net10.0/TodoApp.Tests.dll
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v3.1.4+50e68bbb8b (64-bit .NET 10.0.11)
[xUnit.net 00:00:00.06]   Discovering: TodoApp.Tests
[xUnit.net 00:00:00.09]   Discovered:  TodoApp.Tests
[xUnit.net 00:00:00.11]   Starting:    TodoApp.Tests
[xUnit.net 00:00:01.50]   Finished:    TodoApp.Tests
  TodoApp.Tests test net10.0 succeeded (2.0s)

Test summary: total: 13, failed: 0, succeeded: 13, skipped: 0, duration: 1.9s
Build succeeded in 3.0s
```

## Generating the app

The instructions below were used to generate the app and are included here for reference. You can skip this section if you just want to run the app.

```console
cd /home/bert/Development/dotnet-demo
dotnet new webapp -o TodoApp
cd TodoApp
dotnet add package MySqlConnector
```

Start DB in a podman container

```console
podman pull mariadb:11
podman volume create mariadb-data
podman run -d --name todoappdb -p 3306:3306 -e MARIADB_ROOT_PASSWORD=sekrit -e MARIADB_DATABASE=todo_db -e MARIADB_USER=todo_usr -e MARIADB_PASSWORD=letmeinplz -v mariadb-data:/var/lib/mysql:Z mariadb:11
```

Initialize the database

```console
mariadb -h localhost --port=3306 -utodo_usr -pletmeinplz todo_db << _EOF_
CREATE TABLE IF NOT EXISTS todos (     
    id INT AUTO_INCREMENT PRIMARY KEY,
    title VARCHAR(255) NOT NULL,
    is_done BOOLEAN NOT NULL DEFAULT FALSE,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);                                                        
_EOF_
```

Start the application with `dotnet run` from the `TodoApp` directory and visit <http://localhost:PORT> in your browser (the port number should be visible in the terminal). You should see a page with a list of todos (which will be empty at first).

## Adding tests

```console
cd /home/bert/Development/dotnet-demo
dotnet new xunit -o TodoApp.Tests
cd TodoApp.Tests
dotnet add reference ../TodoApp/TodoApp.csproj
dotnet add package Microsoft.Extensions.Configuration
dotnet add package Microsoft.Extensions.Configuration.Json
cd ..
dotnet new sln
dotnet sln add TodoApp/TodoApp.csproj TodoApp.Tests/TodoApp.Tests.csproj
```

Create a test database in the MariaDB container:

```console
mariadb -h localhost --port=3306 -uroot -psekrit << '_EOF_'
> CREATE DATABASE IF NOT EXISTS todo_test_db;
GRANT ALL PRIVILEGES ON todo_test_db.* TO 'todo_usr'@'%';
FLUSH PRIVILEGES;
_EOF_
mariadb -h localhost --port=3306 -utodo_usr -pletmeinplz todo_test_db << '_EOF_'
CREATE TABLE IF NOT EXISTS todos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    title VARCHAR(255) NOT NULL,
    is_done BOOLEAN NOT NULL DEFAULT FALSE,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);
_EOF_
```
