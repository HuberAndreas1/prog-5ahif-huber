using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Avalonia_DI.ViewModels;

public record TodoItem(string Title, bool IsDone);

public partial class TodoListViewModel : ViewModelBase
{
    
    [ObservableProperty] private string newTodoTitle = "";

    public ObservableCollection<TodoItem> Items { get; } = [
        new("Buy groceries", false),
        new("Learn Avalonia", false),
        new("Finish project", true)
    ];

    [RelayCommand]
    private void AddTodo()
    {
        Items.Add(new TodoItem(NewTodoTitle, false));
    }
}