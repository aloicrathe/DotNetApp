using MySqlConnector;

namespace TodoApp.Data;

public record Todo(int Id, string Title, bool IsDone, DateTime CreatedAt);

public class TodoRepository
{
    private readonly string _connectionString;

    public TodoRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("TodoDb")
            ?? throw new InvalidOperationException("Connection string 'TodoDb' not configured.");
    }

    public async Task<List<Todo>> GetAllAsync()
    {
        var todos = new List<Todo>();

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT id, title, is_done, created_at FROM todos ORDER BY created_at DESC;";

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            todos.Add(new Todo(
                reader.GetInt32("id"),
                reader.GetString("title"),
                reader.GetBoolean("is_done"),
                reader.GetDateTime("created_at")));
        }

        return todos;
    }

    public async Task AddAsync(string title)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO todos (title) VALUES (@title);";
        command.Parameters.AddWithValue("@title", title);

        await command.ExecuteNonQueryAsync();
    }

    public async Task ToggleDoneAsync(int id)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE todos SET is_done = NOT is_done WHERE id = @id;";
        command.Parameters.AddWithValue("@id", id);

        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM todos WHERE id = @id;";
        command.Parameters.AddWithValue("@id", id);

        await command.ExecuteNonQueryAsync();
    }
}