using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TodoApp.Data;

namespace TodoApp.Pages;

public class IndexModel : PageModel
{
    private readonly TodoRepository _repository;

    public IndexModel(TodoRepository repository)
    {
        _repository = repository;
    }

    public List<Todo> Todos { get; set; } = new();

    public async Task OnGetAsync()
    {
        Todos = await _repository.GetAllAsync();
    }

    public async Task<IActionResult> OnPostAddAsync(string title)
    {
        if (!string.IsNullOrWhiteSpace(title))
        {
            await _repository.AddAsync(title);
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleAsync(int id)
    {
        await _repository.ToggleDoneAsync(id);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await _repository.DeleteAsync(id);
        return RedirectToPage();
    }
}
