using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FieldOps.Mobile.Services;

namespace FieldOps.Mobile.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly TokenStore tokens;

    [ObservableProperty] private string token;
    [ObservableProperty] private string status = "";

    public SettingsViewModel(TokenStore tokens)
    {
        this.tokens = tokens;
        token = tokens.Token ?? "";
    }

    [RelayCommand]
    private void Save()
    {
        tokens.Token = Token;
        Status = string.IsNullOrWhiteSpace(Token) ? "Token cleared." : "Saved. Go to the Ask tab.";
    }
}
