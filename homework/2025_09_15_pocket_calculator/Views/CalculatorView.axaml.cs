using _2025_09_15_pocket_calculator.ViewModels;
using Avalonia.Controls;

namespace _2025_09_15_pocket_calculator.Views;

public partial class CalculatorView : UserControl
{
    public CalculatorView()
    {
        InitializeComponent();
        DataContext = new CalculatorViewModel();
    }
}