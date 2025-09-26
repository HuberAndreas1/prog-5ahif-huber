namespace Avalonia_DI.ViewModels;

public partial class MainWindowViewModel(TodoListViewModel todoListViewModel) : ViewModelBase
{
    public TodoListViewModel TodoListViewModel { get; } = todoListViewModel;
}

