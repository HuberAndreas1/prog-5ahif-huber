using System;
using System.Text.RegularExpressions;
using _2025_09_15_pocket_calculator.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace _2025_09_15_pocket_calculator.ViewModels;

public partial class CalculatorViewModel : ViewModelBase
{
    [ObservableProperty] private string displayText = "0";
    
    [RelayCommand]
    private void AddToDisplay(string parameter)
    {
        if (DisplayText == "0")
        {
            DisplayText = parameter switch
            {
                _ when Regex.IsMatch(parameter, @"([0-9]|\+|\-)") => parameter,
                _ => DisplayText + parameter
            };
        }
        else
        { 
            DisplayText += parameter;
        }
    }

    [RelayCommand]
    private void ClearButtonClick()
    {
        DisplayText = "0";
    }

    [RelayCommand]
    private void EqualsButtonClick()
    {
        try
        {
            DisplayText = CalculatorModel.CalculateFromString(DisplayText).ToString();
        }
        catch (Exception e)
        {
            // Launch MessageBox
            var box = MessageBoxManager.GetMessageBoxStandard("Error",
                e.Message,
                ButtonEnum.Ok);
            
            box.ShowAsync();
            ClearButtonClick();
        }
    }
    
    
}