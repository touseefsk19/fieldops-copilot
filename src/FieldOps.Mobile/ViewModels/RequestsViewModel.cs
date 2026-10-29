using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FieldOps.Mobile.Models;
using FieldOps.Mobile.Services;

namespace FieldOps.Mobile.ViewModels;

public partial class RequestsViewModel(FieldOpsApi api, RequestsCache cache) : ObservableObject
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
            await cache.SaveAsync(items);          // keep a copy for when we're offline
            Show(items, isSupervisor);
        }
        catch (ApiException ex) when (ex.IsNetworkError)
        {
            var cached = await cache.LoadAsync();
            if (cached is null)
            {
                Message = ex.Message;              // offline and nothing saved yet
                return;                            // (finally still runs)
            }
            Show(cached.Items, isSupervisor: false);   // offline = read-only: no Approve buttons
            var savedAt = DateTime.SpecifyKind(cached.SavedAtUtc, DateTimeKind.Utc).ToLocalTime();
            Message = $"Offline · showing the list from {savedAt:dd MMM HH:mm}. Approve is off until the API is back.";
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

    // Fills the list. CanApprove only for a supervisor AND a request still waiting.
    private void Show(List<RequestDto> items, bool isSupervisor)
    {
        Requests.Clear();
        foreach (var r in items)
            Requests.Add(new RequestItem(r, isSupervisor && r.Status == "PendingApproval", ApproveCommand));

        if (Requests.Count == 0) Message = "No requests yet.";
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