using TodoApp.Pages;
using Xunit;

namespace TodoApp.Tests;

[Collection("Database collection")]
public class IndexModelTests : IAsyncLifetime
{
    private readonly TodoRepositoryFixture _fixture = new();
    private IndexModel Model => new(_fixture.Repository);

    public Task InitializeAsync() => _fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task OnGetAsync_PopulatesTodosFromRepository()
    {
        await _fixture.Repository.AddAsync("Existing todo");

        var model = Model;
        await model.OnGetAsync();

        Assert.Single(model.Todos);
        Assert.Equal("Existing todo", model.Todos[0].Title);
    }

    [Fact]
    public async Task OnPostAddAsync_WithValidTitle_AddsTodoAndRedirects()
    {
        var model = Model;

        var result = await model.OnPostAddAsync("New task");

        Assert.IsType<Microsoft.AspNetCore.Mvc.RedirectToPageResult>(result);
        var todos = await _fixture.Repository.GetAllAsync();
        Assert.Single(todos);
        Assert.Equal("New task", todos[0].Title);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task OnPostAddAsync_WithBlankTitle_DoesNotAddTodo(string? title)
    {
        var model = Model;

        await model.OnPostAddAsync(title!);

        var todos = await _fixture.Repository.GetAllAsync();
        Assert.Empty(todos);
    }

    [Fact]
    public async Task OnPostToggleAsync_TogglesTodo()
    {
        await _fixture.Repository.AddAsync("Toggle me");
        var todo = (await _fixture.Repository.GetAllAsync()).Single();
        var model = Model;

        await model.OnPostToggleAsync(todo.Id);

        var updated = (await _fixture.Repository.GetAllAsync()).Single();
        Assert.True(updated.IsDone);
    }

    [Fact]
    public async Task OnPostDeleteAsync_RemovesTodo()
    {
        await _fixture.Repository.AddAsync("Delete me");
        var todo = (await _fixture.Repository.GetAllAsync()).Single();
        var model = Model;

        await model.OnPostDeleteAsync(todo.Id);

        var todos = await _fixture.Repository.GetAllAsync();
        Assert.Empty(todos);
    }
}