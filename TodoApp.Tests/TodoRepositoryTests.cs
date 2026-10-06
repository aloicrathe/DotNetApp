using TodoApp.Data;
using Xunit;

namespace TodoApp.Tests;

[Collection("Database collection")]
public class TodoRepositoryTests : IAsyncLifetime
{
    private readonly TodoRepositoryFixture _fixture = new();
    private TodoRepository Repository => _fixture.Repository;

    public Task InitializeAsync() => _fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task AddAsync_InsertsNewTodo()
    {
        await Repository.AddAsync("Buy milk");

        var todos = await Repository.GetAllAsync();

        Assert.Single(todos);
        Assert.Equal("Buy milk", todos[0].Title);
        Assert.False(todos[0].IsDone);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenNoTodos()
    {
        var todos = await Repository.GetAllAsync();

        Assert.Empty(todos);
    }

    [Fact]
    public async Task GetAllAsync_OrdersByCreatedAtDescending()
    {
        await Repository.AddAsync("First");
        await Task.Delay(1100); // ensure distinct CREATED_AT timestamps (second resolution)
        await Repository.AddAsync("Second");

        var todos = await Repository.GetAllAsync();

        Assert.Equal("Second", todos[0].Title);
        Assert.Equal("First", todos[1].Title);
    }

    [Fact]
    public async Task ToggleDoneAsync_FlipsIsDone()
    {
        await Repository.AddAsync("Wash car");
        var todo = (await Repository.GetAllAsync()).Single();

        await Repository.ToggleDoneAsync(todo.Id);
        var afterFirstToggle = (await Repository.GetAllAsync()).Single();
        Assert.True(afterFirstToggle.IsDone);

        await Repository.ToggleDoneAsync(todo.Id);
        var afterSecondToggle = (await Repository.GetAllAsync()).Single();
        Assert.False(afterSecondToggle.IsDone);
    }

    [Fact]
    public async Task DeleteAsync_RemovesTodo()
    {
        await Repository.AddAsync("Temporary");
        var todo = (await Repository.GetAllAsync()).Single();

        await Repository.DeleteAsync(todo.Id);
        var todos = await Repository.GetAllAsync();

        Assert.Empty(todos);
    }
}