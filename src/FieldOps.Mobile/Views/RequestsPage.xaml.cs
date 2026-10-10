using FieldOps.Mobile.ViewModels;

namespace FieldOps.Mobile.Views;

public partial class RequestsPage : ContentPage
{
    private readonly RequestsViewModel vm;

    public RequestsPage(RequestsViewModel vm)
    {
        InitializeComponent();
        BindingContext = this.vm = vm;
    }

    // Reload every time the tab is opened (e.g. after switching token in Settings)
    protected override void OnAppearing()
    {
        base.OnAppearing();
        vm.LoadCommand.Execute(null);
    }
}