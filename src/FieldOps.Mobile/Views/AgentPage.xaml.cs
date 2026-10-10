using FieldOps.Mobile.ViewModels;

namespace FieldOps.Mobile.Views;

public partial class AgentPage : ContentPage
{
    public AgentPage(AgentViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}