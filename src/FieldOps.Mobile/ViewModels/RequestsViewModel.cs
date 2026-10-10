using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FieldOps.Mobile.Services;

namespace FieldOps.Mobile.ViewModels;

public partial class RequestsViewModel(FieldOpsApi api) : ObservableObject
{
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string message = "";
    [ObservableProperty] private string whoAmI = "";

    public ObservableCollection<RequestItem> Requests { get; } = [];

    [RelayCommand]   // → LoadCommand
    private async Task LoadAsync()
    {
        IsBusy = true;
        Message = "";
        try
        {
            // The role decides what the UI SHOWS. The API still decides what is ALLOWED.
            var me = await api.GetMeAsync();
            var isSupervisor = me.Roles.Contains("supervisor");
            WhoAmI = $"Signed in as {me.Name} · {(isSupervisor ? "Supervisor" : "Technician")}";

            var items = await api.GetRequestsAsync();
            Requests.Clear();
            foreach (var r in items)
                Requests.Add(new RequestItem(r, isSupervisor && r.Status == "PendingApproval", ApproveCommand));

            if (Requests.Count == 0) Message = "No requests yet.";
        }
        catch (ApiException ex)
        {
            Message = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]   // → ApproveCommand (IAsyncRelayCommand<RequestItem>)
    private async Task ApproveAsync(RequestItem item)
    {
        try
        {
            var result = await api.ApproveAsync(item.Dto.Id);
            await LoadAsync();                                   // reload first (it clears Message)...
            Message = $"Request #{result.Id} is now {result.Status}.";   // ...then say what happened
        }
        catch (ApiException ex)
        {
            Message = ex.Message;
        }
    }
}