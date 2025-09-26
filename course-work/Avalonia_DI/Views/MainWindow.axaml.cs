using Avalonia_DI.ViewModels;
using Avalonia.Controls;

namespace Avalonia_DI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(MainWindowViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}