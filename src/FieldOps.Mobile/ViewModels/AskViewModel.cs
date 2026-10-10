using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FieldOps.Mobile.Models;
using FieldOps.Mobile.Services;

namespace FieldOps.Mobile.ViewModels;

public partial class AskViewModel(FieldOpsApi api) : ObservableObject
{
    [ObservableProperty] private string question = "";
    [ObservableProperty] private string answer = "";
    [ObservableProperty] private string error = "";
    [ObservableProperty] private bool isBusy;

    public ObservableCollection<Citation> Citations { get; } = [];

    [RelayCommand]   // generates AskCommand for the button
    private async Task AskAsync()
    {
        if (IsBusy || string.IsNullOrWhiteSpace(Question)) return;

        IsBusy = true;
        Error = "";
        Answer = "";
        Citations.Clear();
        try
        {
            var result = await api.AskAsync(Question.Trim());
            Answer = result.Answer;
            foreach (var c in result.Citations) Citations.Add(c);
        }
        catch (ApiException ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}