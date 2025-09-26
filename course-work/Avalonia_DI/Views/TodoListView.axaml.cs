using System.Collections.ObjectModel;
using Avalonia_DI.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Avalonia_DI.Views;

public partial class TodoListView : UserControl
{
    public TodoListView()
    {
        InitializeComponent();
    }

    public TodoListView(TodoListViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}