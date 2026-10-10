using FieldOps.Mobile.ViewModels;

namespace FieldOps.Mobile.Views;

public partial class AskPage : ContentPage
{
    public AskPage(AskViewModel vm)   // injected by DI (registered in MauiProgram)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}