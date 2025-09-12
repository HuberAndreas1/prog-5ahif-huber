using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualBasic.CompilerServices;

namespace _2025_09_12_Avalonia.ViewModels;

public partial class CalculatorViewModel : ViewModelBase
{
    public double FirstNumber { get; set; }
    public double SecondNumber { get; set; }

    [ObservableProperty] private double result;
    public string SelectedOperator { get; set; } = "+";
    public ObservableCollection<string> Operators { get; } = ["+", "-", "*", "/"];

    [RelayCommand]
    private void Calculate()
    {
        Result = SelectedOperator switch
        {
            "+" => FirstNumber + SecondNumber,
            "-" => FirstNumber - SecondNumber,
            "*" => FirstNumber * SecondNumber,
            "/" => SecondNumber != 0 ? FirstNumber / SecondNumber : throw new DivideByZeroException("Cannot divide by zero."),
            _ => throw new InvalidOperationException("Invalid operator.")
        };
    }
}